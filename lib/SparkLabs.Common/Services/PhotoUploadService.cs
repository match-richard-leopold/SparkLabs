using Microsoft.Extensions.Logging;
using SparkLabs.Common.Clients;

namespace SparkLabs.Common.Services;

public interface IPhotoUploadService
{
    /// <summary>
    /// Upload a single photo for a user.
    /// </summary>
    Task<PhotoUploadResult> UploadPhotoAsync(
        string brandId,
        Guid userId,
        byte[] imageBytes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload multiple photos for a user with controlled concurrency.
    /// </summary>
    Task<BatchUploadResult> UploadPhotosAsync(
        string brandId,
        Guid userId,
        IEnumerable<byte[]> images,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all photos for a user.
    /// </summary>
    Task<IEnumerable<PhotoInfo>> GetPhotosAsync(
        string brandId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a photo for a user.
    /// </summary>
    Task DeletePhotoAsync(
        string brandId,
        Guid photoId,
        Guid userId,
        CancellationToken cancellationToken = default);
}

public record BatchUploadResult(
    IReadOnlyList<PhotoUploadResult> Successful,
    IReadOnlyList<BatchUploadError> Failed,
    TimeSpan Duration);

public record BatchUploadError(int Index, string Error);

public class PhotoUploadService : IPhotoUploadService
{
    private readonly IModerationClient _moderationClient;
    private readonly IPhotoApiClient _photoApiClient;
    private readonly ILogger<PhotoUploadService> _logger;

    // Limit concurrent calls to moderation service (it's rate-limited)
    private const int MaxConcurrentModerationCalls = 5;

    public PhotoUploadService(
        IModerationClient moderationClient,
        IPhotoApiClient photoApiClient,
        ILogger<PhotoUploadService> logger)
    {
        _moderationClient = moderationClient;
        _photoApiClient = photoApiClient;
        _logger = logger;
    }

    public async Task<PhotoUploadResult> UploadPhotoAsync(
        string brandId,
        Guid userId,
        byte[] imageBytes,
        CancellationToken cancellationToken = default)
    {
        // Get moderation signature
        var moderation = await _moderationClient.GetSignatureAsync(imageBytes, cancellationToken);

        if (!moderation.Passed)
        {
            throw new ModerationFailedException($"Image failed moderation with score {moderation.Score}");
        }

        // Upload to PhotoApi
        return await _photoApiClient.UploadPhotoAsync(
            brandId, userId, imageBytes, moderation.Signature, cancellationToken);
    }

    public async Task<BatchUploadResult> UploadPhotosAsync(
        string brandId,
        Guid userId,
        IEnumerable<byte[]> images,
        CancellationToken cancellationToken = default)
    {
        var imageList = images.ToList();
        var startTime = DateTime.UtcNow;

        _logger.LogInformation(
            "Starting batch upload of {Count} photos for user {UserId}",
            imageList.Count, userId);

        var successful = new List<PhotoUploadResult>();
        var failed = new List<BatchUploadError>();

        // Use semaphore to limit concurrent moderation calls
        using var semaphore = new SemaphoreSlim(MaxConcurrentModerationCalls);

        var tasks = imageList.Select(async (imageBytes, index) =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var result = await UploadSinglePhotoAsync(
                    brandId, userId, imageBytes, index, cancellationToken);
                return (Index: index, Result: result, Error: (string?)null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to upload photo {Index} for user {UserId}", index, userId);
                return (Index: index, Result: (PhotoUploadResult?)null, Error: ex.Message);
            }
            finally
            {
                semaphore.Release();
            }
        });

        var results = await Task.WhenAll(tasks);

        foreach (var (index, result, error) in results.OrderBy(r => r.Index))
        {
            if (result != null)
                successful.Add(result);
            else
                failed.Add(new BatchUploadError(index, error!));
        }

        var duration = DateTime.UtcNow - startTime;

        _logger.LogInformation(
            "Batch upload completed: {Successful} successful, {Failed} failed, duration {Duration}ms",
            successful.Count, failed.Count, duration.TotalMilliseconds);

        return new BatchUploadResult(successful, failed, duration);
    }

    private async Task<PhotoUploadResult> UploadSinglePhotoAsync(
        string brandId,
        Guid userId,
        byte[] imageBytes,
        int index,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Processing photo {Index} for user {UserId}", index, userId);

        // Get moderation signature (this is the rate-limited call)
        var moderation = await _moderationClient.GetSignatureAsync(imageBytes, cancellationToken);

        if (!moderation.Passed)
        {
            throw new ModerationFailedException($"Image {index} failed moderation with score {moderation.Score}");
        }

        // Upload to PhotoApi
        return await _photoApiClient.UploadPhotoAsync(
            brandId, userId, imageBytes, moderation.Signature, cancellationToken);
    }

    public async Task<IEnumerable<PhotoInfo>> GetPhotosAsync(
        string brandId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _photoApiClient.ListPhotosAsync(brandId, userId, cancellationToken);
    }

    public async Task DeletePhotoAsync(
        string brandId,
        Guid photoId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await _photoApiClient.DeletePhotoAsync(brandId, photoId, userId, cancellationToken);
    }
}

public class ModerationFailedException : Exception
{
    public ModerationFailedException(string message) : base(message) { }
}
