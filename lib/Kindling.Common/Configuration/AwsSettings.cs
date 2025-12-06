namespace Kindling.Common.Configuration;

public class AwsSettings
{
    public const string SectionName = "Aws";

    public string ServiceUrl { get; set; } = "http://localhost:4566";
    public string Region { get; set; } = "us-east-1";
    public string AccessKey { get; set; } = "test";
    public string SecretKey { get; set; } = "test";
    public S3Settings S3 { get; set; } = new();
    public DynamoDbSettings DynamoDb { get; set; } = new();
}

public class S3Settings
{
    public string PhotoBucket { get; set; } = "kindling-photos";
}

public class DynamoDbSettings
{
    public string PhotoMetadataTable { get; set; } = "PhotoMetadata";
}
