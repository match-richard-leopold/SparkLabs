using System.Text.Json;
using SparkLabs.Common.Configuration;
using SparkLabs.Common.Data;
using SparkLabs.Common.Messaging;
using SparkLabs.Common.Models;

namespace SparkLabs.Worker.Handlers;

public class UserInteractionHandler : IMessageHandler
{
    private readonly ILogger<UserInteractionHandler> _logger;
    private readonly IInteractionRepository _interactionRepository;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly KafkaSettings _kafkaSettings;

    public UserInteractionHandler(
        ILogger<UserInteractionHandler> logger,
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
        var interaction = JsonSerializer.Deserialize<UserInteractionEvent>(messageJson);
        if (interaction == null)
        {
            _logger.LogWarning("Failed to deserialize UserInteractionEvent");
            return;
        }

        _logger.LogInformation(
            "Processing interaction: {Type} from {FromUser} to {ToUser}",
            interaction.Type, interaction.FromUserId, interaction.ToUserId);

        // Only check for mutual match on Like interactions
        if (interaction.Type == InteractionType.Like)
        {
            // Check if the other user has already liked this user
            var hasReverseLike = await _interactionRepository.HasLikedAsync(
                interaction.ToUserId,
                interaction.FromUserId);

            // Insert the Like
            await _interactionRepository.CreateAsync(interaction);

            if (hasReverseLike)
            {
                await CreateMutualMatchAsync(interaction);
            }
        }
        else
        {
            // Pass - just insert
            await _interactionRepository.CreateAsync(interaction);
        }
    }

    private async Task CreateMutualMatchAsync(UserInteractionEvent interaction)
    {
        _logger.LogInformation(
            "Mutual match detected between {CausingUser} and {AffectedUser}",
            interaction.FromUserId, interaction.ToUserId);

        var now = DateTime.UtcNow;

        // Insert MutualMatch records for both users
        var matchEvents = new[]
        {
            new UserInteractionEvent
            {
                EventId = UuidV7.NewGuid(),
                FromUserId = interaction.FromUserId,
                ToUserId = interaction.ToUserId,
                Type = InteractionType.MutualMatch,
                BrandId = interaction.BrandId,
                Timestamp = now
            },
            new UserInteractionEvent
            {
                EventId = UuidV7.NewGuid(),
                FromUserId = interaction.ToUserId,
                ToUserId = interaction.FromUserId,
                Type = InteractionType.MutualMatch,
                BrandId = interaction.BrandId,
                Timestamp = now
            }
        };

        await _interactionRepository.CreateBatchAsync(matchEvents);

        // Publish MutualMatch event to notifications topic
        var mutualMatchEvent = new MutualMatchEvent
        {
            EventId = UuidV7.NewGuid(),
            CausingUserId = interaction.FromUserId,
            AffectedUserId = interaction.ToUserId,
            BrandId = interaction.BrandId,
            Timestamp = now
        };

        await _kafkaProducer.PublishAsync(
            _kafkaSettings.Topics.Notifications,
            MessageTypes.MutualMatch,
            mutualMatchEvent,
            $"{interaction.FromUserId}:{interaction.ToUserId}");

        _logger.LogInformation(
            "Published MutualMatch event to {Topic}",
            _kafkaSettings.Topics.Notifications);
    }
}
