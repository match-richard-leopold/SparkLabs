using System.Text.Json;
using SparkLabs.Common.Configuration;
using SparkLabs.Common.Data;
using SparkLabs.Common.Messaging;

namespace SparkLabs.Worker.Handlers;

public class GetMostActiveUsersHandler : IMessageHandler
{
    private readonly ILogger<GetMostActiveUsersHandler> _logger;
    private readonly IInteractionRepository _interactionRepository;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly KafkaSettings _kafkaSettings;

    public GetMostActiveUsersHandler(
        ILogger<GetMostActiveUsersHandler> logger,
        IInteractionRepository interactionRepository,
        IKafkaProducer kafkaProducer,
        KafkaSettings kafkaSettings)
    {
        _logger = logger;
        _interactionRepository = interactionRepository;
        _kafkaProducer = kafkaProducer;
        _kafkaSettings = kafkaSettings;
    }

    public async Task HandleAsync(string messageJson)
    {
        var request = JsonSerializer.Deserialize<GetMostActiveUsersRequest>(messageJson);
        var correlationId = request?.CorrelationId ?? Guid.Empty;
        var limit = request?.Limit ?? 10;
        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        _logger.LogInformation(
            "Processing GetMostActiveUsers request {CorrelationId} - top {Limit} for {Date}",
            correlationId, limit, date);

        var topUsers = await _interactionRepository.GetTopActiveUsersAsync(date, limit);

        var result = new MostActiveUsersResult
        {
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
            Date = date,
            Users = topUsers.Select(u => new ActiveUserEntry
            {
                UserId = u.UserId,
                ActivityCount = u.ActivityCount
            }).ToList()
        };

        await _kafkaProducer.PublishAsync(
            _kafkaSettings.Topics.Notifications,
            MessageTypes.MostActiveUsersResult,
            result,
            correlationId.ToString());

        _logger.LogInformation(
            "Published MostActiveUsersResult for {CorrelationId} with {Count} users to {Topic}",
            correlationId, result.Users.Count, _kafkaSettings.Topics.Notifications);
    }
}
