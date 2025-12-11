using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using SparkLabs.Common.Brands;
using SparkLabs.Common.Configuration;
using SparkLabs.Common.Models;

namespace SparkLabs.Common.Data;

public interface IFlameExtensionRepository
{
    Task<FlameExtension?> GetByUserIdAsync(Guid userId);
    Task SaveAsync(FlameExtension extension);
    Task DeleteAsync(Guid userId);
}

public class FlameExtensionRepository : IFlameExtensionRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;

    public FlameExtensionRepository(IAmazonDynamoDB dynamoDb, AwsSettings awsSettings)
    {
        _dynamoDb = dynamoDb;
        _tableName = awsSettings.DynamoDb.FlameExtensionsTable;
    }

    public async Task<FlameExtension?> GetByUserIdAsync(Guid userId)
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

    public async Task SaveAsync(FlameExtension extension)
    {
        var now = DateTime.UtcNow;
        var item = new Dictionary<string, AttributeValue>
        {
            ["UserId"] = new AttributeValue { S = extension.UserId.ToString() },
            ["RelationshipGoal"] = new AttributeValue { N = ((int)extension.RelationshipGoal).ToString() },
            ["FamilyPlans"] = new AttributeValue { N = ((int)extension.FamilyPlans).ToString() },
            ["PoliticalLeaning"] = new AttributeValue { N = ((int)extension.PoliticalLeaning).ToString() },
            ["Religion"] = new AttributeValue { N = ((int)extension.Religion).ToString() },
            ["WantsPets"] = new AttributeValue { BOOL = extension.WantsPets },
            ["DietaryPreference"] = new AttributeValue { N = ((int)extension.DietaryPreference).ToString() },
            ["UpdatedAt"] = new AttributeValue { S = now.ToString("O") }
        };

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

    private static FlameExtension MapFromDynamoDb(Dictionary<string, AttributeValue> item)
    {
        var extension = new FlameExtension
        {
            UserId = Guid.Parse(item["UserId"].S),
            BrandId = Brand.Flame,
            RelationshipGoal = item.TryGetValue("RelationshipGoal", out var rg) ? (RelationshipGoal)int.Parse(rg.N) : RelationshipGoal.Unknown,
            FamilyPlans = item.TryGetValue("FamilyPlans", out var fp) ? (FamilyPlans)int.Parse(fp.N) : FamilyPlans.Unknown,
            PoliticalLeaning = item.TryGetValue("PoliticalLeaning", out var pl) ? (PoliticalLeaning)int.Parse(pl.N) : PoliticalLeaning.Unknown,
            Religion = item.TryGetValue("Religion", out var r) ? (ReligiousAffiliation)int.Parse(r.N) : ReligiousAffiliation.Unknown,
            WantsPets = item.TryGetValue("WantsPets", out var wp) && wp.BOOL,
            DietaryPreference = item.TryGetValue("DietaryPreference", out var dp) ? (DietaryPreference)int.Parse(dp.N) : DietaryPreference.Unknown
        };

        if (item.TryGetValue("CreatedAt", out var ca))
            extension.CreatedAt = DateTime.Parse(ca.S);

        if (item.TryGetValue("UpdatedAt", out var ua))
            extension.UpdatedAt = DateTime.Parse(ua.S);

        return extension;
    }
}
