namespace SparkLabs.Common.Configuration;

public class KafkaSettings
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = "localhost:9092";
    public string ConsumerGroupId { get; set; } = "sparklabs-worker";
    public KafkaTopics Topics { get; set; } = new();
}

public class KafkaTopics
{
    public string MessageProcessing { get; set; } = "message-processing";
    public string Notifications { get; set; } = "notifications";
}
