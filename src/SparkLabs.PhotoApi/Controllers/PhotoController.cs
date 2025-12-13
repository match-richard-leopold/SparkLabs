using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using SparkLabs.Common.Configuration;
using SparkLabs.Common.Data;
using SparkLabs.Common.Models;

namespace SparkLabs.PhotoApi.Controllers;

[ApiController]
public class PhotoController : ControllerBase
{
    private readonly IPhotoMetadataRepository _metadataRepository;
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly ILogger<PhotoController> _logger;

    private static readonly HashSet<string> ValidBrands = new(StringComparer.OrdinalIgnoreCase)
    {
        "kindling", "spark", "flame"
    };

    public PhotoController(
        IPhotoMetadataRepository metadataRepository,
        IAmazonS3 s3Client,
        AwsSettings awsSettings,
        ILogger<PhotoController> logger)
    {
        _metadataRepository = metadataRepository;
        _s3Client = s3Client;
        _bucketName = awsSettings.S3.PhotoBucket;
        _logger = logger;
    }

    /// <summary>
    /// Upload a photo for a user
    /// Requires X-Moderation-Signature header (validated by middleware)
    /// </summary>
    [HttpPut("{brandId}/users/{userId}/photos")]
    public async Task<IActionResult> UploadPhoto(string brandId, Guid userId)
    {
        if (!ValidBrands.Contains(brandId))
            return BadRequest(new { error = $"Invalid brand: {brandId}" });

        if (!Request.ContentLength.HasValue)
            return BadRequest(new { error = "Content-Length header required" });

        // Generate photo ID
        var photoId = UuidV7.NewGuid();
        var s3Key = $"{brandId}/{photoId}";
        var s3Path = $"s3://{_bucketName}/{s3Key}";

        // Upload to S3
        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = s3Key,
            InputStream = Request.Body,
            ContentType = Request.ContentType ?? "image/jpeg",
            Headers = { ContentLength = Request.ContentLength.Value }
        };

        await _s3Client.PutObjectAsync(putRequest);
        _logger.LogInformation("Uploaded photo {PhotoId} to S3: {S3Path}", photoId, s3Path);

        // Save metadata to DynamoDB
        var metadata = PhotoMetadata.Create(brandId, userId, photoId, s3Path);
        await _metadataRepository.SaveAsync(metadata);
        _logger.LogInformation("Saved photo metadata for user {UserId}, photo {PhotoId}", userId, photoId);

        return Ok(new
        {
            photoId,
            s3Path,
            uploadedAt = metadata.UploadedAt
        });
    }

    /// <summary>
    /// List photos for a user
    /// </summary>
    [HttpGet("{brandId}/users/{userId}/photos")]
    public async Task<IActionResult> ListUserPhotos(string brandId, Guid userId)
    {
        if (!ValidBrands.Contains(brandId))
            return BadRequest(new { error = $"Invalid brand: {brandId}" });

        var photos = await _metadataRepository.GetByUserAsync(brandId, userId);

        return Ok(photos.Select(p => new
        {
            photoId = p.PhotoId,
            s3Path = p.S3Path,
            uploadedAt = p.UploadedAt
        }));
    }

    /// <summary>
    /// Get photo bytes
    /// </summary>
    [HttpGet("{brandId}/photos/{photoId}")]
    public async Task<IActionResult> GetPhoto(string brandId, Guid photoId)
    {
        if (!ValidBrands.Contains(brandId))
            return BadRequest(new { error = $"Invalid brand: {brandId}" });

        var s3Key = $"{brandId}/{photoId}";

        try
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = s3Key
            };

            var response = await _s3Client.GetObjectAsync(getRequest);

            return File(response.ResponseStream, response.Headers.ContentType ?? "image/jpeg");
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound(new { error = "Photo not found" });
        }
    }

    /// <summary>
    /// Soft delete a photo (set isVisible = false)
    /// </summary>
    [HttpDelete("{brandId}/photos/{photoId}")]
    public async Task<IActionResult> DeletePhoto(string brandId, Guid photoId)
    {
        if (!ValidBrands.Contains(brandId))
            return BadRequest(new { error = $"Invalid brand: {brandId}" });

        if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) ||
            !Guid.TryParse(userIdHeader, out var userId))
        {
            return BadRequest(new { error = "Missing or invalid X-User-Id header" });
        }

        await _metadataRepository.SetVisibilityAsync(brandId, userId, photoId, isVisible: false);
        _logger.LogInformation("Soft deleted photo {PhotoId} for user {UserId}", photoId, userId);

        return NoContent();
    }
}
