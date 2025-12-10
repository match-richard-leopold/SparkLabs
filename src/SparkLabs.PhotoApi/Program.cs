using SparkLabs.Common.Telemetry;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSparkLabsTelemetry("SparkLabs.PhotoApi", builder.Configuration);
builder.Logging.AddSparkLabsLogging("SparkLabs.PhotoApi", builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
