using Microsoft.AspNetCore.Mvc;
using SparkLabs.Common.Brands;
using SparkLabs.Common.Data;
using SparkLabs.Common.Models;
using SparkLabs.ProfileApi.Auth;

namespace SparkLabs.ProfileApi.Controllers;

/// <summary>
/// Flame brand profiles (lifestyle & values focused dating)
/// Handles both core profile data (Postgres) and brand extension (DynamoDB)
/// </summary>
[ApiController]
[Route("flame/profiles")]
public class FlameController : ControllerBase
{
    private readonly IProfileRepository _profileRepository;
    private readonly IFlameExtensionRepository _extensionRepository;
    private readonly IUserContext _userContext;
    private readonly ILogger<FlameController> _logger;

    public FlameController(
        IProfileRepository profileRepository,
        IFlameExtensionRepository extensionRepository,
        IUserContext userContext,
        ILogger<FlameController> logger)
    {
        _profileRepository = profileRepository;
        _extensionRepository = extensionRepository;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>
    /// Get the current user's Flame profile
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        var profile = await _profileRepository.GetByIdAsync(_userContext.UserId!.Value);
        if (profile == null || profile.BrandId != Brand.Flame)
            return NotFound();

        var extension = await _extensionRepository.GetByUserIdAsync(_userContext.UserId!.Value);

        return Ok(MapToResponse(profile, extension));
    }

    /// <summary>
    /// Get a Flame profile by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var profile = await _profileRepository.GetByIdAsync(id);
        if (profile == null || profile.BrandId != Brand.Flame)
            return NotFound();

        var extension = await _extensionRepository.GetByUserIdAsync(id);

        return Ok(MapToResponse(profile, extension));
    }

    /// <summary>
    /// List Flame profiles (for discovery)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int limit = 20, [FromQuery] int offset = 0)
    {
        var profiles = await _profileRepository.GetByBrandAsync(Brand.Flame, limit, offset);

        var results = new List<FlameProfileResponse>();
        foreach (var profile in profiles)
        {
            var extension = await _extensionRepository.GetByUserIdAsync(profile.Id);
            results.Add(MapToResponse(profile, extension));
        }

        return Ok(results);
    }

    /// <summary>
    /// Create a new Flame profile
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFlameProfileRequest request)
    {
        var profile = new UserProfile
        {
            BrandId = Brand.Flame,
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

        var extension = new FlameExtension
        {
            UserId = id,
            BrandId = Brand.Flame,
            RelationshipGoal = request.RelationshipGoal,
            FamilyPlans = request.FamilyPlans,
            PoliticalLeaning = request.PoliticalLeaning,
            Religion = request.Religion,
            WantsPets = request.WantsPets,
            DietaryPreference = request.DietaryPreference
        };

        await _extensionRepository.SaveAsync(extension);

        _logger.LogInformation("Created Flame profile {ProfileId}", id);

        return CreatedAtAction(nameof(GetById), new { id }, MapToResponse(profile, extension));
    }

    /// <summary>
    /// Update the current user's Flame profile
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateFlameProfileRequest request)
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        var profile = await _profileRepository.GetByIdAsync(_userContext.UserId!.Value);
        if (profile == null || profile.BrandId != Brand.Flame)
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
            extension = new FlameExtension
            {
                UserId = _userContext.UserId!.Value,
                BrandId = Brand.Flame
            };
        }

        extension.RelationshipGoal = request.RelationshipGoal ?? extension.RelationshipGoal;
        extension.FamilyPlans = request.FamilyPlans ?? extension.FamilyPlans;
        extension.PoliticalLeaning = request.PoliticalLeaning ?? extension.PoliticalLeaning;
        extension.Religion = request.Religion ?? extension.Religion;
        extension.WantsPets = request.WantsPets ?? extension.WantsPets;
        extension.DietaryPreference = request.DietaryPreference ?? extension.DietaryPreference;

        await _extensionRepository.SaveAsync(extension);

        _logger.LogInformation("Updated Flame profile {ProfileId}", profile.Id);

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
        if (profile == null || profile.BrandId != Brand.Flame)
            return NotFound();

        await _profileRepository.DeleteAsync(_userContext.UserId!.Value);
        await _extensionRepository.DeleteAsync(_userContext.UserId!.Value);

        _logger.LogInformation("Deleted Flame profile {ProfileId}", _userContext.UserId);

        return NoContent();
    }

    private static FlameProfileResponse MapToResponse(UserProfile profile, FlameExtension? extension)
    {
        return new FlameProfileResponse(
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
            extension?.RelationshipGoal ?? RelationshipGoal.Unknown,
            extension?.FamilyPlans ?? FamilyPlans.Unknown,
            extension?.PoliticalLeaning ?? PoliticalLeaning.Unknown,
            extension?.Religion ?? ReligiousAffiliation.Unknown,
            extension?.WantsPets ?? false,
            extension?.DietaryPreference ?? DietaryPreference.Unknown
        );
    }
}

// Request/Response DTOs

public record CreateFlameProfileRequest(
    string Email,
    string DisplayName,
    DateOnly DateOfBirth,
    string? Bio,
    string? Location,
    Gender Gender,
    Gender SeekGender,
    RelationshipGoal RelationshipGoal,
    FamilyPlans FamilyPlans,
    PoliticalLeaning PoliticalLeaning,
    ReligiousAffiliation Religion,
    bool WantsPets,
    DietaryPreference DietaryPreference
);

public record UpdateFlameProfileRequest(
    string? DisplayName,
    string? Bio,
    string? Location,
    Gender? Gender,
    Gender? SeekGender,
    RelationshipGoal? RelationshipGoal,
    FamilyPlans? FamilyPlans,
    PoliticalLeaning? PoliticalLeaning,
    ReligiousAffiliation? Religion,
    bool? WantsPets,
    DietaryPreference? DietaryPreference
);

public record FlameProfileResponse(
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
    RelationshipGoal RelationshipGoal,
    FamilyPlans FamilyPlans,
    PoliticalLeaning PoliticalLeaning,
    ReligiousAffiliation Religion,
    bool WantsPets,
    DietaryPreference DietaryPreference
);
