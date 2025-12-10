namespace SparkLabs.Common.Configuration;

public class TelemetrySettings
{
    public const string SectionName = "Telemetry";

    public string OtlpEndpoint { get; set; } = "http://localhost:4317";
}
