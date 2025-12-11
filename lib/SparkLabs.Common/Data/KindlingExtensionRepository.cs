using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using SparkLabs.Common.Brands;
using SparkLabs.Common.Configuration;
using SparkLabs.Common.Models;

namespace SparkLabs.Common.Data;

public interface IKindlingExtensionRepository
{
    Task<KindlingExtension?> GetByUserIdAsync(Guid userId);
    Task SaveAsync(KindlingExtension extension);
    Task DeleteAsync(Guid userId);
}

public class KindlingExtensionRepository : IKindlingExtensionRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;

    public KindlingExtensionRepository(IAmazonDynamoDB dynamoDb, AwsSettings awsSettings)
    {
        _dynamoDb = dynamoDb;
        _tableName = awsSettings.DynamoDb.KindlingExtensionsTable;
    }

    public async Task<KindlingExtension?> GetByUserIdAsync(Guid userId)
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

    public async Task SaveAsync(KindlingExtension extension)
    {
        var now = DateTime.UtcNow;
        var item = new Dictionary<string, AttributeValue>
        {
            ["UserId"] = new AttributeValue { S = extension.UserId.ToString() },
            ["SunSign"] = new AttributeValue { N = ((int)extension.SunSign).ToString() },
            ["BelievesInAstrology"] = new AttributeValue { BOOL = extension.BelievesInAstrology },
            ["UpdatedAt"] = new AttributeValue { S = now.ToString("O") }
        };

        if (extension.RisingSign.HasValue)
            item["RisingSign"] = new AttributeValue { N = ((int)extension.RisingSign.Value).ToString() };

        if (extension.MoonSign.HasValue)
            item["MoonSign"] = new AttributeValue { N = ((int)extension.MoonSign.Value).ToString() };

        if (extension.CompatibleSigns.Count > 0)
            item["CompatibleSigns"] = new AttributeValue { NS = extension.CompatibleSigns.Select(s => ((int)s).ToString()).ToList() };

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

    private static KindlingExtension MapFromDynamoDb(Dictionary<string, AttributeValue> item)
    {
        var extension = new KindlingExtension
        {
            UserId = Guid.Parse(item["UserId"].S),
            BrandId = Brand.Kindling,
            SunSign = (ZodiacSign)int.Parse(item["SunSign"].N),
            BelievesInAstrology = item.TryGetValue("BelievesInAstrology", out var ba) && ba.BOOL
        };

        if (item.TryGetValue("RisingSign", out var rs))
            extension.RisingSign = (ZodiacSign)int.Parse(rs.N);

        if (item.TryGetValue("MoonSign", out var ms))
            extension.MoonSign = (ZodiacSign)int.Parse(ms.N);

        if (item.TryGetValue("CompatibleSigns", out var cs) && cs.NS != null)
            extension.CompatibleSigns = cs.NS.Select(n => (ZodiacSign)int.Parse(n)).ToList();

        if (item.TryGetValue("CreatedAt", out var ca))
            extension.CreatedAt = DateTime.Parse(ca.S);

        if (item.TryGetValue("UpdatedAt", out var ua))
            extension.UpdatedAt = DateTime.Parse(ua.S);

        return extension;
    }
}
