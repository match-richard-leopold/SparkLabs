using SparkLabs.Common.Configuration;
using SparkLabs.Common.Data;
using SparkLabs.Common.Messaging;
using SparkLabs.Common.Telemetry;
using SparkLabs.Worker;
using SparkLabs.Worker.Handlers;

var builder = Host.CreateApplicationBuilder(args);

// Telemetry
builder.Services.AddSparkLabsTelemetry("SparkLabs.Worker", builder.Configuration);
builder.Logging.AddSparkLabsLogging("SparkLabs.Worker", builder.Configuration);

// Configuration
var kafkaSettings = builder.Configuration.GetSection(KafkaSettings.SectionName).Get<KafkaSettings>()!;
builder.Services.AddSingleton(kafkaSettings);

// Database
var connectionString = builder.Configuration.GetConnectionString("ProfileDb")!;
builder.Services.AddSingleton<IDbConnectionFactory>(new NpgsqlConnectionFactory(connectionString));
builder.Services.AddScoped<IInteractionRepository, InteractionRepository>();

// Kafka
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

// Message Handlers
builder.Services.AddScoped<UserInteractionHandler>();
builder.Services.AddScoped<GetMostActiveUsersHandler>();

// Worker
builder.Services.AddHostedService<MessageProcessingWorker>();

var host = builder.Build();
host.Run();
