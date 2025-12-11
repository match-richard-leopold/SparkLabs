using Microsoft.AspNetCore.Mvc;
using SparkLabs.Common.Brands;
using SparkLabs.Common.Data;
using SparkLabs.Common.Models;
using SparkLabs.ProfileApi.Auth;

namespace SparkLabs.ProfileApi.Controllers;

/// <summary>
/// Kindling brand profiles (astrology-focused dating)
/// Handles both core profile data (Postgres) and brand extension (DynamoDB)
/// </summary>
[ApiController]
[Route("kindling/profiles")]
public class KindlingController : ControllerBase
{
    private readonly IProfileRepository _profileRepository;
    private readonly IKindlingExtensionRepository _extensionRepository;
    private readonly IUserContext _userContext;
    private readonly ILogger<KindlingController> _logger;

    public KindlingController(
        IProfileRepository profileRepository,
        IKindlingExtensionRepository extensionRepository,
        IUserContext userContext,
        ILogger<KindlingController> logger)
    {
        _profileRepository = profileRepository;
        _extensionRepository = extensionRepository;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>
    /// Get the current user's Kindling profile
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        var profile = await _profileRepository.GetByIdAsync(_userContext.UserId!.Value);
        if (profile == null || profile.BrandId != Brand.Kindling)
            return NotFound();

        var extension = await _extensionRepository.GetByUserIdAsync(_userContext.UserId!.Value);

        return Ok(MapToResponse(profile, extension));
    }

    /// <summary>
    /// Get a Kindling profile by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var profile = await _profileRepository.GetByIdAsync(id);
        if (profile == null || profile.BrandId != Brand.Kindling)
            return NotFound();

        var extension = await _extensionRepository.GetByUserIdAsync(id);

        return Ok(MapToResponse(profile, extension));
    }

    /// <summary>
    /// List Kindling profiles (for discovery)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int limit = 20, [FromQuery] int offset = 0)
    {
        var profiles = await _profileRepository.GetByBrandAsync(Brand.Kindling, limit, offset);

        var results = new List<KindlingProfileResponse>();
        foreach (var profile in profiles)
        {
            var extension = await _extensionRepository.GetByUserIdAsync(profile.Id);
            results.Add(MapToResponse(profile, extension));
        }

        return Ok(results);
    }

    /// <summary>
    /// Create a new Kindling profile
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateKindlingProfileRequest request)
    {
        var profile = new UserProfile
        {
            BrandId = Brand.Kindling,
            Email = request.Email,
            DisplayName = request.DisplayName,
            DateOfBirth = request.DateOfBirth,
            Bio = request.Bio,
            Location = request.Location,
            Gender = request.Gender,
            SeekGender = request.SeekGender
        };

        var id = await _profileRepository.CreateAsync(profile);
        profile.Id = id;

        var extension = new KindlingExtension
        {
            UserId = id,
            BrandId = Brand.Kindling,
            SunSign = request.SunSign,
            RisingSign = request.RisingSign,
            MoonSign = request.MoonSign,
            BelievesInAstrology = request.BelievesInAstrology,
            CompatibleSigns = request.CompatibleSigns ?? []
        };

        await _extensionRepository.SaveAsync(extension);

        _logger.LogInformation("Created Kindling profile {ProfileId}", id);

        return CreatedAtAction(nameof(GetById), new { id }, MapToResponse(profile, extension));
    }

    /// <summary>
    /// Update the current user's Kindling profile
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateKindlingProfileRequest request)
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        var profile = await _profileRepository.GetByIdAsync(_userContext.UserId!.Value);
        if (profile == null || profile.BrandId != Brand.Kindling)
            return NotFound();

        // Update core profile fields
        profile.DisplayName = request.DisplayName ?? profile.DisplayName;
        profile.Bio = request.Bio ?? profile.Bio;
        profile.Location = request.Location ?? profile.Location;
        profile.Gender = request.Gender ?? profile.Gender;
        profile.SeekGender = request.SeekGender ?? profile.SeekGender;

        await _profileRepository.UpdateAsync(profile);

        // Update extension if any extension fields provided
        var extension = await _extensionRepository.GetByUserIdAsync(_userContext.UserId!.Value);
        if (extension == null)
        {
            extension = new KindlingExtension
            {
                UserId = _userContext.UserId!.Value,
                BrandId = Brand.Kindling
            };
        }

        extension.SunSign = request.SunSign ?? extension.SunSign;
        extension.RisingSign = request.RisingSign ?? extension.RisingSign;
        extension.MoonSign = request.MoonSign ?? extension.MoonSign;
        extension.BelievesInAstrology = request.BelievesInAstrology ?? extension.BelievesInAstrology;
        extension.CompatibleSigns = request.CompatibleSigns ?? extension.CompatibleSigns;

        await _extensionRepository.SaveAsync(extension);

        _logger.LogInformation("Updated Kindling profile {ProfileId}", profile.Id);

        return Ok(MapToResponse(profile, extension));
    }

    /// <summary>
    /// Delete the current user's profile (soft delete)
    /// </summary>
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMyProfile()
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        var profile = await _profileRepository.GetByIdAsync(_userContext.UserId!.Value);
        if (profile == null || profile.BrandId != Brand.Kindling)
            return NotFound();

        await _profileRepository.DeleteAsync(_userContext.UserId!.Value);
        await _extensionRepository.DeleteAsync(_userContext.UserId!.Value);

        _logger.LogInformation("Deleted Kindling profile {ProfileId}", _userContext.UserId);

        return NoContent();
    }

    private static KindlingProfileResponse MapToResponse(UserProfile profile, KindlingExtension? extension)
    {
        return new KindlingProfileResponse(
            profile.Id,
            profile.Email,
            profile.DisplayName,
            profile.DateOfBirth,
            profile.Bio,
            profile.Location,
            profile.Gender,
            profile.SeekGender,
            profile.CreatedAt,
            profile.UpdatedAt,
            extension?.SunSign ?? ZodiacSign.Unknown,
            extension?.RisingSign,
            extension?.MoonSign,
            extension?.BelievesInAstrology ?? false,
            extension?.CompatibleSigns ?? []
        );
    }
}

// Request/Response DTOs

public record CreateKindlingProfileRequest(
    string Email,
    string DisplayName,
    DateOnly DateOfBirth,
    string? Bio,
    string? Location,
    Gender Gender,
    Gender SeekGender,
    ZodiacSign SunSign,
    ZodiacSign? RisingSign,
    ZodiacSign? MoonSign,
    bool BelievesInAstrology,
    List<ZodiacSign>? CompatibleSigns
);

public record UpdateKindlingProfileRequest(
    string? DisplayName,
    string? Bio,
    string? Location,
    Gender? Gender,
    Gender? SeekGender,
    ZodiacSign? SunSign,
    ZodiacSign? RisingSign,
    ZodiacSign? MoonSign,
    bool? BelievesInAstrology,
    List<ZodiacSign>? CompatibleSigns
);

public record KindlingProfileResponse(
    Guid Id,
    string Email,
    string DisplayName,
    DateOnly DateOfBirth,
    string? Bio,
    string? Location,
    Gender Gender,
    Gender SeekGender,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    ZodiacSign SunSign,
    ZodiacSign? RisingSign,
    ZodiacSign? MoonSign,
    bool BelievesInAstrology,
    List<ZodiacSign> CompatibleSigns
);
