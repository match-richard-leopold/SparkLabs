using SparkLabs.Common.Clients;

namespace SparkLabs.ProfileApi.Tests.Fakes;

/// <summary>
/// Fake photo API client that returns immediate success without actual S3/DynamoDB calls.
/// </summary>
public class FakePhotoApiClient : IPhotoApiClient
{
    private readonly TimeSpan _delay;
    private int _uploadCount;
    private readonly List<PhotoUploadResult> _uploads = new();

    public FakePhotoApiClient(TimeSpan delay = default)
    {
        _delay = delay;
    }

    public int UploadCount => _uploadCount;
    public IReadOnlyList<PhotoUploadResult> Uploads => _uploads;

    public async Task<PhotoUploadResult> UploadPhotoAsync(
        string brandId,
        Guid userId,
        byte[] imageBytes,
        string moderationSignature,
        CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _uploadCount);

        if (_delay > TimeSpan.Zero)
            await Task.Delay(_delay, cancellationToken);

        var result = new PhotoUploadResult(
            PhotoId: Guid.NewGuid(),
            S3Path: $"s3://sparklabs-photos/{brandId}/{Guid.NewGuid()}",
            UploadedAt: DateTime.UtcNow);

        lock (_uploads)
        {
            _uploads.Add(result);
        }

        return result;
    }

    public Task<IEnumerable<PhotoInfo>> ListPhotosAsync(
        string brandId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var photos = _uploads.Select(u => new PhotoInfo(u.PhotoId, u.S3Path, u.UploadedAt));
        return Task.FromResult(photos);
    }

    public Task<byte[]> GetPhotoAsync(
        string brandId,
        Guid photoId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }); // JPEG header
    }

    public Task DeletePhotoAsync(
        string brandId,
        Guid photoId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
