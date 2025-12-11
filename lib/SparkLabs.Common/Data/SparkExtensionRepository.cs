using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using SparkLabs.Common.Brands;
using SparkLabs.Common.Configuration;
using SparkLabs.Common.Models;

namespace SparkLabs.Common.Data;

public interface ISparkExtensionRepository
{
    Task<SparkExtension?> GetByUserIdAsync(Guid userId);
    Task SaveAsync(SparkExtension extension);
    Task DeleteAsync(Guid userId);
}

public class SparkExtensionRepository : ISparkExtensionRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;

    public SparkExtensionRepository(IAmazonDynamoDB dynamoDb, AwsSettings awsSettings)
    {
        _dynamoDb = dynamoDb;
        _tableName = awsSettings.DynamoDb.SparkExtensionsTable;
    }

    public async Task<SparkExtension?> GetByUserIdAsync(Guid userId)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["UserId"] = new AttributeValue { S = userId.ToString() }
            }
        };

        var response = await _dynamoDb.GetItemAsync(request);

        if (response.Item == null || response.Item.Count == 0)
            return null;

        return MapFromDynamoDb(response.Item);
    }

    public async Task SaveAsync(SparkExtension extension)
    {
        var now = DateTime.UtcNow;
        var item = new Dictionary<string, AttributeValue>
        {
            ["UserId"] = new AttributeValue { S = extension.UserId.ToString() },
            ["ActivityLevel"] = new AttributeValue { N = ((int)extension.ActivityLevel).ToString() },
            ["WeekendStyle"] = new AttributeValue { N = ((int)extension.WeekendStyle).ToString() },
            ["OpenToNewHobbies"] = new AttributeValue { BOOL = extension.OpenToNewHobbies },
            ["UpdatedAt"] = new AttributeValue { S = now.ToString("O") }
        };

        if (extension.Hobbies.Count > 0)
            item["Hobbies"] = new AttributeValue { SS = extension.Hobbies };

        if (extension.FavoriteActivities.Count > 0)
            item["FavoriteActivities"] = new AttributeValue { SS = extension.FavoriteActivities };

        // Set CreatedAt only if it's a new record
        var existing = await GetByUserIdAsync(extension.UserId);
        item["CreatedAt"] = new AttributeValue { S = (existing?.CreatedAt ?? now).ToString("O") };

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDb.PutItemAsync(request);
    }

    public async Task DeleteAsync(Guid userId)
    {
        var request = new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["UserId"] = new AttributeValue { S = userId.ToString() }
            }
        };

        await _dynamoDb.DeleteItemAsync(request);
    }

    private static SparkExtension MapFromDynamoDb(Dictionary<string, AttributeValue> item)
    {
        var extension = new SparkExtension
        {
            UserId = Guid.Parse(item["UserId"].S),
            BrandId = Brand.Spark,
            ActivityLevel = item.TryGetValue("ActivityLevel", out var al) ? (ActivityLevel)int.Parse(al.N) : ActivityLevel.Unknown,
            WeekendStyle = item.TryGetValue("WeekendStyle", out var ws) ? (WeekendStyle)int.Parse(ws.N) : WeekendStyle.Unknown,
            OpenToNewHobbies = item.TryGetValue("OpenToNewHobbies", out var onh) && onh.BOOL
        };

        if (item.TryGetValue("Hobbies", out var h) && h.SS != null)
            extension.Hobbies = h.SS;

        if (item.TryGetValue("FavoriteActivities", out var fa) && fa.SS != null)
            extension.FavoriteActivities = fa.SS;

        if (item.TryGetValue("CreatedAt", out var ca))
            extension.CreatedAt = DateTime.Parse(ca.S);

        if (item.TryGetValue("UpdatedAt", out var ua))
            extension.UpdatedAt = DateTime.Parse(ua.S);

        return extension;
    }
}
