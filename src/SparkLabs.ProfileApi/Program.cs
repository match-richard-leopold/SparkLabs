using Amazon.DynamoDBv2;
using SparkLabs.Common.Configuration;
using SparkLabs.Common.Data;
using SparkLabs.Common.Messaging;
using SparkLabs.Common.Services;
using SparkLabs.Common.Telemetry;
using SparkLabs.ProfileApi.Auth;

var builder = WebApplication.CreateBuilder(args);

// Telemetry
builder.Services.AddSparkLabsTelemetry("SparkLabs.ProfileApi", builder.Configuration);
builder.Logging.AddSparkLabsLogging("SparkLabs.ProfileApi", builder.Configuration);

// Configuration
var kafkaSettings = builder.Configuration.GetSection(KafkaSettings.SectionName).Get<KafkaSettings>()!;
builder.Services.AddSingleton(kafkaSettings);

var awsSettings = builder.Configuration.GetSection(AwsSettings.SectionName).Get<AwsSettings>()!;
builder.Services.AddSingleton(awsSettings);

// Database (PostgreSQL - core profiles)
var connectionString = builder.Configuration.GetConnectionString("ProfileDb")!;
builder.Services.AddSingleton<IDbConnectionFactory>(new NpgsqlConnectionFactory(connectionString));
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IInteractionRepository, InteractionRepository>();

// DynamoDB (brand extensions)
builder.Services.AddSingleton<IAmazonDynamoDB>(_ =>
{
    var config = new AmazonDynamoDBConfig
    {
        ServiceURL = awsSettings.ServiceUrl
    };
    return new AmazonDynamoDBClient(awsSettings.AccessKey, awsSettings.SecretKey, config);
});
builder.Services.AddScoped<IKindlingExtensionRepository, KindlingExtensionRepository>();
builder.Services.AddScoped<ISparkExtensionRepository, SparkExtensionRepository>();
builder.Services.AddScoped<IFlameExtensionRepository, FlameExtensionRepository>();

// Services (business logic)
builder.Services.AddScoped<IKindlingProfileService, KindlingProfileService>();
builder.Services.AddScoped<ISparkProfileService, SparkProfileService>();
builder.Services.AddScoped<IFlameProfileService, FlameProfileService>();

// Kafka
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

// Auth (impersonation for dev/testing - see Auth/ImpersonationMiddleware.cs)
builder.Services.AddScoped<IUserContext, UserContext>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseImpersonation();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
