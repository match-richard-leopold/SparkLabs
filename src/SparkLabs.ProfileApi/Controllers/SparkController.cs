using Microsoft.AspNetCore.Mvc;
using SparkLabs.Common.Brands;
using SparkLabs.Common.Data;
using SparkLabs.Common.Models;
using SparkLabs.ProfileApi.Auth;

namespace SparkLabs.ProfileApi.Controllers;

/// <summary>
/// Spark brand profiles (hobbies & activities focused dating)
/// Handles both core profile data (Postgres) and brand extension (DynamoDB)
/// </summary>
[ApiController]
[Route("spark/profiles")]
public class SparkController : ControllerBase
{
    private readonly IProfileRepository _profileRepository;
    private readonly ISparkExtensionRepository _extensionRepository;
    private readonly IUserContext _userContext;
    private readonly ILogger<SparkController> _logger;

    public SparkController(
        IProfileRepository profileRepository,
        ISparkExtensionRepository extensionRepository,
        IUserContext userContext,
        ILogger<SparkController> logger)
    {
        _profileRepository = profileRepository;
        _extensionRepository = extensionRepository;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>
    /// Get the current user's Spark profile
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        var profile = await _profileRepository.GetByIdAsync(_userContext.UserId!.Value);
        if (profile == null || profile.BrandId != Brand.Spark)
            return NotFound();

        var extension = await _extensionRepository.GetByUserIdAsync(_userContext.UserId!.Value);

        return Ok(MapToResponse(profile, extension));
    }

    /// <summary>
    /// Get a Spark profile by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var profile = await _profileRepository.GetByIdAsync(id);
        if (profile == null || profile.BrandId != Brand.Spark)
            return NotFound();

        var extension = await _extensionRepository.GetByUserIdAsync(id);

        return Ok(MapToResponse(profile, extension));
    }

    /// <summary>
    /// List Spark profiles (for discovery)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int limit = 20, [FromQuery] int offset = 0)
    {
        var profiles = await _profileRepository.GetByBrandAsync(Brand.Spark, limit, offset);

        var results = new List<SparkProfileResponse>();
        foreach (var profile in profiles)
        {
            var extension = await _extensionRepository.GetByUserIdAsync(profile.Id);
            results.Add(MapToResponse(profile, extension));
        }

        return Ok(results);
    }

    /// <summary>
    /// Create a new Spark profile
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSparkProfileRequest request)
    {
        var profile = new UserProfile
        {
            BrandId = Brand.Spark,
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

        var extension = new SparkExtension
        {
            UserId = id,
            BrandId = Brand.Spark,
            Hobbies = request.Hobbies ?? [],
            ActivityLevel = request.ActivityLevel,
            WeekendStyle = request.WeekendStyle,
            FavoriteActivities = request.FavoriteActivities ?? [],
            OpenToNewHobbies = request.OpenToNewHobbies
        };

        await _extensionRepository.SaveAsync(extension);

        _logger.LogInformation("Created Spark profile {ProfileId}", id);

        return CreatedAtAction(nameof(GetById), new { id }, MapToResponse(profile, extension));
    }

    /// <summary>
    /// Update the current user's Spark profile
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateSparkProfileRequest request)
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        var profile = await _profileRepository.GetByIdAsync(_userContext.UserId!.Value);
        if (profile == null || profile.BrandId != Brand.Spark)
            return NotFound();

        // Update core profile fields
        profile.DisplayName = request.DisplayName ?? profile.DisplayName;
        profile.Bio = request.Bio ?? profile.Bio;
        profile.Location = request.Location ?? profile.Location;
        profile.Gender = request.Gender ?? profile.Gender;
        profile.SeekGender = request.SeekGender ?? profile.SeekGender;

        await _profileRepository.UpdateAsync(profile);

        // Update extension
        var extension = await _extensionRepository.GetByUserIdAsync(_userContext.UserId!.Value);
        if (extension == null)
        {
            extension = new SparkExtension
            {
                UserId = _userContext.UserId!.Value,
                BrandId = Brand.Spark
            };
        }

        extension.Hobbies = request.Hobbies ?? extension.Hobbies;
        extension.ActivityLevel = request.ActivityLevel ?? extension.ActivityLevel;
        extension.WeekendStyle = request.WeekendStyle ?? extension.WeekendStyle;
        extension.FavoriteActivities = request.FavoriteActivities ?? extension.FavoriteActivities;
        extension.OpenToNewHobbies = request.OpenToNewHobbies ?? extension.OpenToNewHobbies;

        await _extensionRepository.SaveAsync(extension);

        _logger.LogInformation("Updated Spark profile {ProfileId}", profile.Id);

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
        if (profile == null || profile.BrandId != Brand.Spark)
            return NotFound();

        await _profileRepository.DeleteAsync(_userContext.UserId!.Value);
        await _extensionRepository.DeleteAsync(_userContext.UserId!.Value);

        _logger.LogInformation("Deleted Spark profile {ProfileId}", _userContext.UserId);

        return NoContent();
    }

    private static SparkProfileResponse MapToResponse(UserProfile profile, SparkExtension? extension)
    {
        return new SparkProfileResponse(
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
            extension?.Hobbies ?? [],
            extension?.ActivityLevel ?? ActivityLevel.Unknown,
            extension?.WeekendStyle ?? WeekendStyle.Unknown,
            extension?.FavoriteActivities ?? [],
            extension?.OpenToNewHobbies ?? true
        );
    }
}

// Request/Response DTOs

public record CreateSparkProfileRequest(
    string Email,
    string DisplayName,
    DateOnly DateOfBirth,
    string? Bio,
    string? Location,
    Gender Gender,
    Gender SeekGender,
    List<string>? Hobbies,
    ActivityLevel ActivityLevel,
    WeekendStyle WeekendStyle,
    List<string>? FavoriteActivities,
    bool OpenToNewHobbies
);

public record UpdateSparkProfileRequest(
    string? DisplayName,
    string? Bio,
    string? Location,
    Gender? Gender,
    Gender? SeekGender,
    List<string>? Hobbies,
    ActivityLevel? ActivityLevel,
    WeekendStyle? WeekendStyle,
    List<string>? FavoriteActivities,
    bool? OpenToNewHobbies
);

public record SparkProfileResponse(
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
    List<string> Hobbies,
    ActivityLevel ActivityLevel,
    WeekendStyle WeekendStyle,
    List<string> FavoriteActivities,
    bool OpenToNewHobbies
);
