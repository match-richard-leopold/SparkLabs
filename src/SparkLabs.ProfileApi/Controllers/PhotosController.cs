using Microsoft.AspNetCore.Mvc;
using SparkLabs.Common.Clients;
using SparkLabs.Common.Services;
using SparkLabs.ProfileApi.Auth;

namespace SparkLabs.ProfileApi.Controllers;

[ApiController]
[Route("{brandId}/photos")]
public class PhotosController : ControllerBase
{
    private readonly IPhotoUploadService _photoUploadService;
    private readonly IUserContext _userContext;
    private readonly ILogger<PhotosController> _logger;

    private static readonly HashSet<string> ValidBrands = new(StringComparer.OrdinalIgnoreCase)
    {
        "kindling", "spark", "flame"
    };

    public PhotosController(
        IPhotoUploadService photoUploadService,
        IUserContext userContext,
        ILogger<PhotosController> logger)
    {
        _photoUploadService = photoUploadService;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>
    /// Upload a single photo for the current user
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UploadPhoto(string brandId, CancellationToken cancellationToken)
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        if (!ValidBrands.Contains(brandId))
            return BadRequest(new { error = $"Invalid brand: {brandId}" });

        // Read image bytes from request body
        using var ms = new MemoryStream();
        await Request.Body.CopyToAsync(ms, cancellationToken);
        var imageBytes = ms.ToArray();

        if (imageBytes.Length == 0)
            return BadRequest(new { error = "No image data provided" });

        var result = await _photoUploadService.UploadPhotoAsync(
            brandId, _userContext.UserId!.Value, imageBytes, cancellationToken);

        _logger.LogInformation("User {UserId} uploaded photo {PhotoId}", _userContext.UserId, result.PhotoId);

        return Ok(result);
    }

    /// <summary>
    /// Upload multiple photos for the current user (batch)
    /// </summary>
    [HttpPost("batch")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPhotosBatch(
        string brandId,
        [FromForm] List<IFormFile> photos,
        CancellationToken cancellationToken)
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        if (!ValidBrands.Contains(brandId))
            return BadRequest(new { error = $"Invalid brand: {brandId}" });

        if (photos == null || photos.Count == 0)
            return BadRequest(new { error = "No photos provided" });

        // Read all images into memory
        var images = new List<byte[]>();
        foreach (var photo in photos)
        {
            using var ms = new MemoryStream();
            await photo.CopyToAsync(ms, cancellationToken);
            images.Add(ms.ToArray());
        }

        _logger.LogInformation(
            "User {UserId} starting batch upload of {Count} photos",
            _userContext.UserId, images.Count);

        var result = await _photoUploadService.UploadPhotosAsync(
            brandId, _userContext.UserId!.Value, images, cancellationToken);

        return Ok(new
        {
            successful = result.Successful.Count,
            failed = result.Failed.Count,
            durationMs = result.Duration.TotalMilliseconds,
            photos = result.Successful,
            errors = result.Failed
        });
    }

    /// <summary>
    /// Get all photos for the current user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyPhotos(string brandId, CancellationToken cancellationToken)
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        if (!ValidBrands.Contains(brandId))
            return BadRequest(new { error = $"Invalid brand: {brandId}" });

        var photos = await _photoUploadService.GetPhotosAsync(
            brandId, _userContext.UserId!.Value, cancellationToken);

        return Ok(photos);
    }

    /// <summary>
    /// Delete a photo
    /// </summary>
    [HttpDelete("{photoId}")]
    public async Task<IActionResult> DeletePhoto(
        string brandId,
        Guid photoId,
        CancellationToken cancellationToken)
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        if (!ValidBrands.Contains(brandId))
            return BadRequest(new { error = $"Invalid brand: {brandId}" });

        await _photoUploadService.DeletePhotoAsync(
            brandId, photoId, _userContext.UserId!.Value, cancellationToken);

        _logger.LogInformation("User {UserId} deleted photo {PhotoId}", _userContext.UserId, photoId);

        return NoContent();
    }
}
