using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using SparkLabs.Common.Configuration;
using SparkLabs.Common.Models;

namespace SparkLabs.Common.Data;

public interface IPhotoMetadataRepository
{
    Task<PhotoMetadata?> GetByIdAsync(string brandId, Guid photoId);
    Task<IEnumerable<PhotoMetadata>> GetByUserAsync(string brandId, Guid userId, bool visibleOnly = true);
    Task SaveAsync(PhotoMetadata metadata);
    Task SetVisibilityAsync(string brandId, Guid userId, Guid photoId, bool isVisible);
    Task DeleteAsync(string brandId, Guid userId, Guid photoId);
}

public class PhotoMetadataRepository : IPhotoMetadataRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;

    public PhotoMetadataRepository(IAmazonDynamoDB dynamoDb, AwsSettings awsSettings)
    {
        _dynamoDb = dynamoDb;
        _tableName = awsSettings.DynamoDb.PhotoMetadataTable;
    }

    public async Task<PhotoMetadata?> GetByIdAsync(string brandId, Guid photoId)
    {
        // We need to scan/query since we don't have userId
        // In production, you'd use a GSI on photoId
        var request = new ScanRequest
        {
            TableName = _tableName,
            FilterExpression = "begins_with(pk, :brand) AND sk = :photoId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":brand"] = new AttributeValue { S = brandId },
                [":photoId"] = new AttributeValue { S = photoId.ToString() }
            }
        };

        var response = await _dynamoDb.ScanAsync(request);
        var item = response.Items.FirstOrDefault();

        return item == null ? null : MapFromDynamoDb(item);
    }

    public async Task<IEnumerable<PhotoMetadata>> GetByUserAsync(string brandId, Guid userId, bool visibleOnly = true)
    {
        var pk = $"{brandId}#{userId}";

        var request = new QueryRequest
        {
            TableName = _tableName,
            KeyConditionExpression = "pk = :pk",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":pk"] = new AttributeValue { S = pk }
            }
        };

        if (visibleOnly)
        {
            request.FilterExpression = "isVisible = :visible";
            request.ExpressionAttributeValues[":visible"] = new AttributeValue { BOOL = true };
        }

        var response = await _dynamoDb.QueryAsync(request);
        return response.Items.Select(MapFromDynamoDb);
    }

    public async Task SaveAsync(PhotoMetadata metadata)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["pk"] = new AttributeValue { S = metadata.Pk },
            ["sk"] = new AttributeValue { S = metadata.Sk },
            ["s3Path"] = new AttributeValue { S = metadata.S3Path },
            ["isVisible"] = new AttributeValue { BOOL = metadata.IsVisible },
            ["uploadedAt"] = new AttributeValue { S = metadata.UploadedAt.ToString("O") }
        };

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDb.PutItemAsync(request);
    }

    public async Task SetVisibilityAsync(string brandId, Guid userId, Guid photoId, bool isVisible)
    {
        var request = new UpdateItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["pk"] = new AttributeValue { S = $"{brandId}#{userId}" },
                ["sk"] = new AttributeValue { S = photoId.ToString() }
            },
            UpdateExpression = "SET isVisible = :visible",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":visible"] = new AttributeValue { BOOL = isVisible }
            }
        };

        await _dynamoDb.UpdateItemAsync(request);
    }

    public async Task DeleteAsync(string brandId, Guid userId, Guid photoId)
    {
        var request = new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["pk"] = new AttributeValue { S = $"{brandId}#{userId}" },
                ["sk"] = new AttributeValue { S = photoId.ToString() }
            }
        };

        await _dynamoDb.DeleteItemAsync(request);
    }

    private static PhotoMetadata MapFromDynamoDb(Dictionary<string, AttributeValue> item)
    {
        return new PhotoMetadata
        {
            Pk = item["pk"].S,
            Sk = item["sk"].S,
            S3Path = item["s3Path"].S,
            IsVisible = item.TryGetValue("isVisible", out var vis) && vis.BOOL,
            UploadedAt = item.TryGetValue("uploadedAt", out var ua)
                ? DateTime.Parse(ua.S)
                : DateTime.UtcNow
        };
    }
}
