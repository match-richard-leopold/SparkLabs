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
/// Fake implementation that always returns a passing score with minimal delay.
/// Use this to bypass the real moderation service for testing.
/// </summary>
public class FakeModerationClient : IModerationClient
{
    private readonly ILogger<FakeModerationClient> _logger;

    public FakeModerationClient(ILogger<FakeModerationClient> logger)
    {
        _logger = logger;
    }

    public async Task<ModerationResult> GetSignatureAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        // Minimal delay, always passes
        await Task.Delay(50, cancellationToken);

        const int score = 85;
        var payload = new { moderationScore = score };
        var json = JsonSerializer.Serialize(payload);
        var signature = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        _logger.LogDebug("Fake moderation: generated signature with score {Score}", score);

        return new ModerationResult(signature, score, Passed: score >= 70);
    }
}

/// <summary>
/// Real implementation that calls the external moderation service.
/// The service has rate limiting (max 5 concurrent) and ~400ms latency.
/// </summary>
public class ModerationClient : IModerationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ModerationClient> _logger;

    public ModerationClient(HttpClient httpClient, ILogger<ModerationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ModerationResult> GetSignatureAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        using var content = new ByteArrayContent(imageBytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

        var response = await _httpClient.PostAsync("/moderate", content, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            // Log the rate limit header if present
            if (response.Headers.TryGetValues("X-RateLimit-Concurrent-Max", out var values))
            {
                _logger.LogWarning("Moderation rate limited. Max concurrent: {Max}", values.FirstOrDefault());
            }
            throw new ModerationRateLimitException("Moderation service rate limit exceeded");
        }

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<ModerationResponse>(json, JsonOptions);

        _logger.LogDebug("Moderation result: score={Score}, passed={Passed}", result!.Score, result.Passed);

        return new ModerationResult(result.Signature, result.Score, result.Passed);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private record ModerationResponse(string Signature, int Score, bool Passed);
}

public class ModerationRateLimitException : Exception
{
    public ModerationRateLimitException(string message) : base(message) { }
}
