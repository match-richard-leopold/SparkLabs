using System.Text;
using Confluent.Kafka;
using SparkLabs.Common.Configuration;
using SparkLabs.Common.Messaging;
using SparkLabs.Worker.Handlers;

namespace SparkLabs.Worker;

public class MessageProcessingWorker : BackgroundService
{
    private readonly ILogger<MessageProcessingWorker> _logger;
    private readonly KafkaSettings _kafkaSettings;
    private readonly IServiceScopeFactory _scopeFactory;

    public MessageProcessingWorker(
        ILogger<MessageProcessingWorker> logger,
        KafkaSettings kafkaSettings,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _kafkaSettings = kafkaSettings;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            GroupId = _kafkaSettings.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();

        consumer.Subscribe(_kafkaSettings.Topics.MessageProcessing);
        _logger.LogInformation("Subscribed to topic: {Topic}", _kafkaSettings.Topics.MessageProcessing);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(stoppingToken);

                if (consumeResult?.Message == null)
                    continue;

                var messageType = GetMessageType(consumeResult.Message.Headers);
                _logger.LogDebug("Received message type: {MessageType}", messageType);

                await ProcessMessageAsync(messageType, consumeResult.Message.Value);

                consumer.Commit(consumeResult);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Error consuming message");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
            }
        }

        consumer.Close();
    }

    private async Task ProcessMessageAsync(string messageType, string messageJson)
    {
        using var scope = _scopeFactory.CreateScope();

        switch (messageType)
        {
            case MessageTypes.UserInteraction:
                var interactionHandler = scope.ServiceProvider.GetRequiredService<UserInteractionHandler>();
                await interactionHandler.HandleAsync(messageJson);
                break;

            case MessageTypes.GetMostActiveUsers:
                var activeUsersHandler = scope.ServiceProvider.GetRequiredService<GetMostActiveUsersHandler>();
                await activeUsersHandler.HandleAsync(messageJson);
                break;

            default:
                _logger.LogWarning("Unknown message type: {MessageType}", messageType);
                break;
        }
    }

    private static string GetMessageType(Headers headers)
    {
        var header = headers.FirstOrDefault(h => h.Key == "message-type");
        return header != null ? Encoding.UTF8.GetString(header.GetValueBytes()) : string.Empty;
    }
}
