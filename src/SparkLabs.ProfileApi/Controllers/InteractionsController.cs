using Microsoft.AspNetCore.Mvc;
using SparkLabs.Common.Configuration;
using SparkLabs.Common.Data;
using SparkLabs.Common.Messaging;
using SparkLabs.Common.Models;
using SparkLabs.ProfileApi.Auth;

namespace SparkLabs.ProfileApi.Controllers;

[ApiController]
[Route("[controller]")]
public class InteractionsController : ControllerBase
{
    private readonly IInteractionRepository _interactionRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly KafkaSettings _kafkaSettings;
    private readonly IUserContext _userContext;
    private readonly ILogger<InteractionsController> _logger;

    public InteractionsController(
        IInteractionRepository interactionRepository,
        IProfileRepository profileRepository,
        IKafkaProducer kafkaProducer,
        KafkaSettings kafkaSettings,
        IUserContext userContext,
        ILogger<InteractionsController> logger)
    {
        _interactionRepository = interactionRepository;
        _profileRepository = profileRepository;
        _kafkaProducer = kafkaProducer;
        _kafkaSettings = kafkaSettings;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>
    /// Like a user (swipe right)
    /// </summary>
    [HttpPost("like/{targetUserId:guid}")]
    public async Task<IActionResult> Like(Guid targetUserId)
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        var fromUserId = _userContext.UserId!.Value;

        // Get current user's profile to determine brand
        var fromProfile = await _profileRepository.GetByIdAsync(fromUserId);
        if (fromProfile == null)
            return BadRequest("User profile not found");

        var interaction = new UserInteractionEvent
        {
            EventId = UuidV7.NewGuid(),
            FromUserId = fromUserId,
            ToUserId = targetUserId,
            Type = InteractionType.Like,
            BrandId = fromProfile.BrandId,
            Timestamp = DateTime.UtcNow
        };

        // Publish to Kafka for async processing
        await _kafkaProducer.PublishAsync(
            _kafkaSettings.Topics.MessageProcessing,
            MessageTypes.UserInteraction,
            interaction,
            fromUserId.ToString());

        _logger.LogInformation("Published Like from {FromUser} to {ToUser}", fromUserId, targetUserId);

        return Accepted(new { message = "Like recorded", eventId = interaction.EventId });
    }

    /// <summary>
    /// Pass on a user (swipe left)
    /// </summary>
    [HttpPost("pass/{targetUserId:guid}")]
    public async Task<IActionResult> Pass(Guid targetUserId)
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        var fromUserId = _userContext.UserId!.Value;

        var fromProfile = await _profileRepository.GetByIdAsync(fromUserId);
        if (fromProfile == null)
            return BadRequest("User profile not found");

        var interaction = new UserInteractionEvent
        {
            EventId = UuidV7.NewGuid(),
            FromUserId = fromUserId,
            ToUserId = targetUserId,
            Type = InteractionType.Pass,
            BrandId = fromProfile.BrandId,
            Timestamp = DateTime.UtcNow
        };

        await _kafkaProducer.PublishAsync(
            _kafkaSettings.Topics.MessageProcessing,
            MessageTypes.UserInteraction,
            interaction,
            fromUserId.ToString());

        _logger.LogInformation("Published Pass from {FromUser} to {ToUser}", fromUserId, targetUserId);

        return Accepted(new { message = "Pass recorded", eventId = interaction.EventId });
    }

    /// <summary>
    /// Get the current user's matches
    /// </summary>
    [HttpGet("matches")]
    public async Task<IActionResult> GetMatches()
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        var matches = await _interactionRepository.GetMatchesAsync(_userContext.UserId!.Value);
        return Ok(matches);
    }

    /// <summary>
    /// Get the current user's interaction history
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int limit = 100, [FromQuery] int offset = 0)
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        var interactions = await _interactionRepository.GetByUserAsync(_userContext.UserId!.Value, limit, offset);
        return Ok(interactions);
    }

}
