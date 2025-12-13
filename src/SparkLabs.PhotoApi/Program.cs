using Amazon.DynamoDBv2;
using Amazon.S3;
using SparkLabs.Common.Configuration;
using SparkLabs.Common.Data;
using SparkLabs.Common.Telemetry;
using SparkLabs.PhotoApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Telemetry
builder.Services.AddSparkLabsTelemetry("SparkLabs.PhotoApi", builder.Configuration);
builder.Logging.AddSparkLabsLogging("SparkLabs.PhotoApi", builder.Configuration);

// Configuration
var awsSettings = builder.Configuration.GetSection(AwsSettings.SectionName).Get<AwsSettings>()!;
builder.Services.AddSingleton(awsSettings);

// AWS Services (LocalStack)
builder.Services.AddSingleton<IAmazonDynamoDB>(_ =>
{
    var config = new AmazonDynamoDBConfig
    {
        ServiceURL = awsSettings.ServiceUrl
    };
    return new AmazonDynamoDBClient(awsSettings.AccessKey, awsSettings.SecretKey, config);
});

builder.Services.AddSingleton<IAmazonS3>(_ =>
{
    var config = new AmazonS3Config
    {
        ServiceURL = awsSettings.ServiceUrl,
        ForcePathStyle = true // Required for LocalStack
    };
    return new AmazonS3Client(awsSettings.AccessKey, awsSettings.SecretKey, config);
});

// Repositories
builder.Services.AddScoped<IPhotoMetadataRepository, PhotoMetadataRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Moderation signature validation (before routing)
app.UseModerationSignature();

app.UseAuthorization();

app.MapControllers();

app.Run();
