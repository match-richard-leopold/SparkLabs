using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using SparkLabs.Common.Configuration;

namespace SparkLabs.Common.Messaging;

public interface IKafkaProducer : IDisposable
{
    Task PublishAsync<T>(string topic, string messageType, T message, string? key = null);
}

public class KafkaProducer : IKafkaProducer
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducer(KafkaSettings settings)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync<T>(string topic, string messageType, T message, string? key = null)
    {
        var kafkaMessage = new Message<string, string>
        {
            Key = key ?? Guid.NewGuid().ToString(),
            Value = JsonSerializer.Serialize(message),
            Headers = new Headers
            {
                { "message-type", Encoding.UTF8.GetBytes(messageType) }
            }
        };

        await _producer.ProduceAsync(topic, kafkaMessage);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}
