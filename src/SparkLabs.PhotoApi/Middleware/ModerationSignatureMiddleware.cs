using System.Text;
using System.Text.Json;

namespace SparkLabs.PhotoApi.Middleware;

public class ModerationSignatureMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ModerationSignatureMiddleware> _logger;

    private const string HeaderName = "X-Moderation-Signature";
    private const int MinimumScore = 70;

    public ModerationSignatureMiddleware(RequestDelegate next, ILogger<ModerationSignatureMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only validate on PUT requests (photo uploads)
        if (context.Request.Method == HttpMethods.Put)
        {
            if (!context.Request.Headers.TryGetValue(HeaderName, out var signatureHeader))
            {
                _logger.LogWarning("Missing {Header} header", HeaderName);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = $"Missing {HeaderName} header" });
                return;
            }

            var signature = signatureHeader.ToString();
            var validationResult = ValidateSignature(signature);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid moderation signature: {Reason}", validationResult.Reason);
                context.Response.StatusCode = validationResult.StatusCode;
                await context.Response.WriteAsJsonAsync(new { error = validationResult.Reason });
                return;
            }

            _logger.LogDebug("Moderation signature valid, score: {Score}", validationResult.Score);
        }

        await _next(context);
    }

    // in real life this would probably be way more complicated...
    private ValidationResult ValidateSignature(string signature)
    {
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(signature));
            var payload = JsonSerializer.Deserialize<ModerationPayload>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payload == null)
            {
                return ValidationResult.Invalid("Invalid signature format", StatusCodes.Status400BadRequest);
            }

            if (payload.ModerationScore < MinimumScore)
            {
                return ValidationResult.Invalid(
                    $"Moderation score {payload.ModerationScore} below minimum threshold {MinimumScore}",
                    StatusCodes.Status422UnprocessableEntity);
            }

            return ValidationResult.Valid(payload.ModerationScore);
        }
        catch (FormatException)
        {
            return ValidationResult.Invalid("Invalid base64 encoding", StatusCodes.Status400BadRequest);
        }
        catch (JsonException)
        {
            return ValidationResult.Invalid("Invalid JSON in signature", StatusCodes.Status400BadRequest);
        }
    }

    private record ModerationPayload(int ModerationScore);

    private record ValidationResult(bool IsValid, int Score, string? Reason, int StatusCode)
    {
        public static ValidationResult Valid(int score) => new(true, score, null, 200);
        public static ValidationResult Invalid(string reason, int statusCode) => new(false, 0, reason, statusCode);
    }
}

public static class ModerationSignatureMiddlewareExtensions
{
    public static IApplicationBuilder UseModerationSignature(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ModerationSignatureMiddleware>();
    }
}
