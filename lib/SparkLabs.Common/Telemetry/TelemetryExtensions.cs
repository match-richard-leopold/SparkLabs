using SparkLabs.Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SparkLabs.Common.Telemetry;

public static class TelemetryExtensions
{
    private const string DefaultOtlpEndpoint = "http://localhost:4317";

    public static IServiceCollection AddSparkLabsTelemetry(
        this IServiceCollection services,
        string serviceName,
        IConfiguration configuration)
    {
        var settings = configuration.GetSection(TelemetrySettings.SectionName).Get<TelemetrySettings>()
            ?? new TelemetrySettings();

        var endpoint = settings.OtlpEndpoint;

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName)
            .AddAttributes(new[]
            {
                new KeyValuePair<string, object>("deployment.environment", "development")
            });

        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource(serviceName)
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(endpoint);
                    });
            })
            .WithMetrics(builder =>
            {
                builder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(serviceName)
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(endpoint);
                    });
            });

        return services;
    }

    public static ILoggingBuilder AddSparkLabsLogging(
        this ILoggingBuilder builder,
        string serviceName,
        IConfiguration configuration)
    {
        var settings = configuration.GetSection(TelemetrySettings.SectionName).Get<TelemetrySettings>()
            ?? new TelemetrySettings();

        var endpoint = settings.OtlpEndpoint;

        builder.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService(serviceName));

            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;

            options.AddOtlpExporter(exporterOptions =>
            {
                exporterOptions.Endpoint = new Uri(endpoint);
            });
        });

        return builder;
    }
}
