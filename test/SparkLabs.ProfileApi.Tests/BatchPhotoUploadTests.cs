using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SparkLabs.ProfileApi.Tests;

public class BatchPhotoUploadTests : IClassFixture<ProfileApiFactory>
{
    private readonly ProfileApiFactory _factory;
    private readonly HttpClient _client;

    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public BatchPhotoUploadTests(ProfileApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task BatchUpload_20Photos_CompletesWithin2Seconds()
    {
        // Arrange: 20 photos, 100ms moderation delay, semaphore of 5
        // Expected: ~4 batches of 5 = 4 * 100ms = 400ms (with some overhead)
        // SLA: Must complete in under 2 seconds
        const int photoCount = 20;
        var fakeImageBytes = CreateFakeJpegBytes();

        using var content = new MultipartFormDataContent();
        for (int i = 0; i < photoCount; i++)
        {
            var imageContent = new ByteArrayContent(fakeImageBytes);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(imageContent, "photos", $"photo{i}.jpg");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "/kindling/photos/batch")
        {
            Content = content
        };
        request.Headers.Add("X-Impersonate-User", TestUserId.ToString());

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.SendAsync(request);
        stopwatch.Stop();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(json);

        var successful = result.RootElement.GetProperty("successful").GetInt32();
        var failed = result.RootElement.GetProperty("failed").GetInt32();
        var durationMs = result.RootElement.GetProperty("durationMs").GetDouble();

        Assert.Equal(photoCount, successful);
        Assert.Equal(0, failed);

        // SLA check: must complete within 2 seconds
        Assert.True(stopwatch.ElapsedMilliseconds < 2000,
            $"Batch upload took {stopwatch.ElapsedMilliseconds}ms, expected < 2000ms");

        // Verify moderation was called for each photo
        Assert.Equal(photoCount, _factory.ModerationClient.CallCount);
    }

    [Fact]
    public async Task BatchUpload_SinglePhoto_Succeeds()
    {
        // Arrange
        var fakeImageBytes = CreateFakeJpegBytes();

        using var content = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(fakeImageBytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(imageContent, "photos", "single.jpg");

        var request = new HttpRequestMessage(HttpMethod.Post, "/kindling/photos/batch")
        {
            Content = content
        };
        request.Headers.Add("X-Impersonate-User", TestUserId.ToString());

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(json);

        Assert.Equal(1, result.RootElement.GetProperty("successful").GetInt32());
        Assert.Equal(0, result.RootElement.GetProperty("failed").GetInt32());
    }

    [Fact]
    public async Task BatchUpload_NoAuth_ReturnsUnauthorized()
    {
        // Arrange
        var fakeImageBytes = CreateFakeJpegBytes();

        using var content = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(fakeImageBytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(imageContent, "photos", "photo.jpg");

        // Note: no X-Impersonate-User header
        var request = new HttpRequestMessage(HttpMethod.Post, "/kindling/photos/batch")
        {
            Content = content
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task BatchUpload_InvalidBrand_ReturnsBadRequest()
    {
        // Arrange
        var fakeImageBytes = CreateFakeJpegBytes();

        using var content = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(fakeImageBytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(imageContent, "photos", "photo.jpg");

        var request = new HttpRequestMessage(HttpMethod.Post, "/invalid-brand/photos/batch")
        {
            Content = content
        };
        request.Headers.Add("X-Impersonate-User", TestUserId.ToString());

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static byte[] CreateFakeJpegBytes()
    {
        // Minimal valid JPEG header + some padding
        return new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46,
            0x49, 0x46, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01,
            0x00, 0x01, 0x00, 0x00, 0xFF, 0xD9
        };
    }
}
