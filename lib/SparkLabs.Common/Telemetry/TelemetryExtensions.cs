using SparkLabs.Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenTelemetry.Instrumentation.AWS;
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

        var resourceBuilder = ResourceBuilder.CreateEmpty()
            .AddService(serviceName, serviceInstanceId: Environment.MachineName)
            .AddAttributes([
                new KeyValuePair<string, object>("deployment.environment", "development")
            ]);

        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            foreach (var header in request.Headers)
                            {
                                if (header.Key.StartsWith("X-", StringComparison.OrdinalIgnoreCase))
                                {
                                    activity.SetTag($"http.request.header.{header.Key.ToLowerInvariant()}", string.Join(",", header.Value.ToArray()));
                                }
                            }
                        };
                        options.EnrichWithHttpResponse = (activity, response) =>
                        {
                            foreach (var header in response.Headers)
                            {
                                if (header.Key.StartsWith("X-", StringComparison.OrdinalIgnoreCase))
                                {
                                    activity.SetTag($"http.response.header.{header.Key.ToLowerInvariant()}", string.Join(",", header.Value.ToArray()));
                                }
                            }
                        };
                    })
                    .AddHttpClientInstrumentation()
                    .AddNpgsql()
                    .AddAWSInstrumentation()
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

        builder.ClearProviders();
        builder.SetMinimumLevel(LogLevel.Information);
        builder.AddConsole();
        builder.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(ResourceBuilder.CreateEmpty()
                .AddService(serviceName, serviceInstanceId: Environment.MachineName));

            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;

            options.AddOtlpExporter(exporterOptions =>
            {
                exporterOptions.Endpoint = new Uri(endpoint);
            });
        });

        return builder;
    }
}
