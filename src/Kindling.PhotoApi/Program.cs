using Kindling.Common.Telemetry;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKindlingTelemetry("Kindling.PhotoApi", builder.Configuration);
builder.Logging.AddKindlingLogging("Kindling.PhotoApi", builder.Configuration);

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
