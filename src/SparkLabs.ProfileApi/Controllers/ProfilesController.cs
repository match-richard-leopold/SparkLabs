using Microsoft.AspNetCore.Mvc;
using SparkLabs.Common.Data;
using SparkLabs.Common.Models;
using SparkLabs.ProfileApi.Auth;

namespace SparkLabs.ProfileApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ProfilesController : ControllerBase
{
    private readonly IProfileRepository _profileRepository;
    private readonly IUserContext _userContext;
    private readonly ILogger<ProfilesController> _logger;

    public ProfilesController(
        IProfileRepository profileRepository,
        IUserContext userContext,
        ILogger<ProfilesController> logger)
    {
        _profileRepository = profileRepository;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>
    /// Get the current user's profile
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        var profile = await _profileRepository.GetByIdAsync(_userContext.UserId!.Value);
        if (profile == null)
            return NotFound();

        return Ok(profile);
    }

    /// <summary>
    /// Get a profile by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var profile = await _profileRepository.GetByIdAsync(id);
        if (profile == null)
            return NotFound();

        return Ok(profile);
    }

    /// <summary>
    /// Get profiles by brand
    /// </summary>
    [HttpGet("brand/{brand}")]
    public async Task<IActionResult> GetByBrand(Brand brand, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
    {
        var profiles = await _profileRepository.GetByBrandAsync(brand, limit, offset);
        return Ok(profiles);
    }

    /// <summary>
    /// Create a new profile
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProfileRequest request)
    {
        var profile = new UserProfile
        {
            BrandId = request.BrandId,
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

        return CreatedAtAction(nameof(GetById), new { id }, profile);
    }

    /// <summary>
    /// Update the current user's profile
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        var profile = await _profileRepository.GetByIdAsync(_userContext.UserId!.Value);
        if (profile == null)
            return NotFound();

        profile.DisplayName = request.DisplayName ?? profile.DisplayName;
        profile.Bio = request.Bio ?? profile.Bio;
        profile.Location = request.Location ?? profile.Location;
        profile.Gender = request.Gender ?? profile.Gender;
        profile.SeekGender = request.SeekGender ?? profile.SeekGender;

        await _profileRepository.UpdateAsync(profile);

        return Ok(profile);
    }

    /// <summary>
    /// Delete the current user's profile (soft delete)
    /// </summary>
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMyProfile()
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        var deleted = await _profileRepository.DeleteAsync(_userContext.UserId!.Value);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

public record CreateProfileRequest(
    Brand BrandId,
    string Email,
    string DisplayName,
    DateOnly DateOfBirth,
    string? Bio,
    string? Location,
    Gender Gender,
    Gender SeekGender
);

public record UpdateProfileRequest(
    string? DisplayName,
    string? Bio,
    string? Location,
    Gender? Gender,
    Gender? SeekGender
);
