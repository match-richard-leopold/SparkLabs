using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SparkLabs.Common.Clients;

public interface IModerationClient
{
    /// <summary>
    /// Get a moderation signature for an image.
    /// The signature is required by PhotoApi to upload photos.
    /// </summary>
    Task<ModerationResult> GetSignatureAsync(byte[] imageBytes, CancellationToken cancellationToken = default);
}

public record ModerationResult(string Signature, int Score, bool Passed);

/// <summary>
/// Stub implementation that always returns a passing score.
/// In production, this would call an external moderation service.
/// </summary>
public class ModerationClient : IModerationClient
{
    private readonly ILogger<ModerationClient> _logger;

    public ModerationClient(ILogger<ModerationClient> logger)
    {
        _logger = logger;
    }

    public async Task<ModerationResult> GetSignatureAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        // TODO: In production, this would:
        // 1. Call external moderation service with image bytes
        // 2. Service analyzes image for inappropriate content
        // 3. Returns score (0-100) and signature for PhotoApi

        // For now, simulate a small delay and return passing score
        await Task.Delay(50, cancellationToken);

        const int score = 85;
        var payload = new { moderationScore = score };
        var json = JsonSerializer.Serialize(payload);
        var signature = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        _logger.LogDebug("Generated moderation signature with score {Score}", score);

        return new ModerationResult(signature, score, Passed: score >= 70);
    }
}
