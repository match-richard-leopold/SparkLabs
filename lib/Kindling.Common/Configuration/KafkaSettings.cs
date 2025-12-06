namespace Kindling.Common.Configuration;

public class KafkaSettings
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = "localhost:9092";
    public KafkaTopics Topics { get; set; } = new();
}

public class KafkaTopics
{
    public string PhotoProcessing { get; set; } = "photo-processing";
}
