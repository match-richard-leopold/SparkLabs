using System.Text;
using System.Text.Json;
using SparkLabs.Common.Clients;

namespace SparkLabs.ProfileApi.Tests.Fakes;

/// <summary>
/// Fake moderation client with configurable delay to simulate rate-limited external service.
/// </summary>
public class FakeModerationClient : IModerationClient
{
    private readonly TimeSpan _delay;
    private readonly int _score;
    private int _callCount;

    public FakeModerationClient(TimeSpan delay, int score = 85)
    {
        _delay = delay;
        _score = score;
    }

    public int CallCount => _callCount;

    public async Task<ModerationResult> GetSignatureAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _callCount);

        // Simulate external service latency
        await Task.Delay(_delay, cancellationToken);

        var payload = new { moderationScore = _score };
        var json = JsonSerializer.Serialize(payload);
        var signature = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        return new ModerationResult(signature, _score, Passed: _score >= 70);
    }
}
