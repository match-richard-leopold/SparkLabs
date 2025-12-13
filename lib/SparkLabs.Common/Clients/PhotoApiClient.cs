using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SparkLabs.Common.Clients;

public interface IPhotoApiClient
{
    Task<PhotoUploadResult> UploadPhotoAsync(
        string brandId,
        Guid userId,
        byte[] imageBytes,
        string moderationSignature,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<PhotoInfo>> ListPhotosAsync(
        string brandId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<byte[]> GetPhotoAsync(
        string brandId,
        Guid photoId,
        CancellationToken cancellationToken = default);

    Task DeletePhotoAsync(
        string brandId,
        Guid photoId,
        Guid userId,
        CancellationToken cancellationToken = default);
}

public record PhotoUploadResult(Guid PhotoId, string S3Path, DateTime UploadedAt);
public record PhotoInfo(Guid PhotoId, string S3Path, DateTime UploadedAt);

public class PhotoApiClient : IPhotoApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PhotoApiClient> _logger;

    public PhotoApiClient(HttpClient httpClient, ILogger<PhotoApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PhotoUploadResult> UploadPhotoAsync(
        string brandId,
        Guid userId,
        byte[] imageBytes,
        string moderationSignature,
        CancellationToken cancellationToken = default)
    {
        var url = $"{brandId}/users/{userId}/photos";

        using var content = new ByteArrayContent(imageBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Headers.ContentLength = imageBytes.Length;

        using var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Content = content;
        request.Headers.Add("X-Moderation-Signature", moderationSignature);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<PhotoUploadResponse>(json, JsonOptions);

        _logger.LogDebug("Uploaded photo {PhotoId} for user {UserId}", result!.PhotoId, userId);

        return new PhotoUploadResult(result.PhotoId, result.S3Path, result.UploadedAt);
    }

    public async Task<IEnumerable<PhotoInfo>> ListPhotosAsync(
        string brandId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var url = $"{brandId}/users/{userId}/photos";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var photos = JsonSerializer.Deserialize<List<PhotoInfoResponse>>(json, JsonOptions);

        return photos!.Select(p => new PhotoInfo(p.PhotoId, p.S3Path, p.UploadedAt));
    }

    public async Task<byte[]> GetPhotoAsync(
        string brandId,
        Guid photoId,
        CancellationToken cancellationToken = default)
    {
        var url = $"{brandId}/photos/{photoId}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public async Task DeletePhotoAsync(
        string brandId,
        Guid photoId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var url = $"{brandId}/photos/{photoId}";

        using var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Add("X-User-Id", userId.ToString());

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger.LogDebug("Deleted photo {PhotoId} for user {UserId}", photoId, userId);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private record PhotoUploadResponse(Guid PhotoId, string S3Path, DateTime UploadedAt);
    private record PhotoInfoResponse(Guid PhotoId, string S3Path, DateTime UploadedAt);
}
