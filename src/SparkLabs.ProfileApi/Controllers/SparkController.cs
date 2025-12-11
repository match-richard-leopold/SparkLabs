using Microsoft.AspNetCore.Mvc;
using SparkLabs.Common.Services;
using SparkLabs.ProfileApi.Auth;

namespace SparkLabs.ProfileApi.Controllers;

[ApiController]
[Route("spark/profiles")]
public class SparkController : ControllerBase
{
    private readonly ISparkProfileService _profileService;
    private readonly IUserContext _userContext;
    private readonly ILogger<SparkController> _logger;

    public SparkController(
        ISparkProfileService profileService,
        IUserContext userContext,
        ILogger<SparkController> logger)
    {
        _profileService = profileService;
        _userContext = userContext;
        _logger = logger;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        var profile = await _profileService.GetByIdAsync(_userContext.UserId!.Value);
        if (profile == null)
            return NotFound();

        return Ok(profile);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var profile = await _profileService.GetByIdAsync(id);
        if (profile == null)
            return NotFound();

        return Ok(profile);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int limit = 20, [FromQuery] int offset = 0)
    {
        var profiles = await _profileService.ListAsync(limit, offset);
        return Ok(profiles);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSparkProfile request)
    {
        var profile = await _profileService.CreateAsync(request);
        _logger.LogInformation("Created Spark profile {ProfileId}", profile.Id);
        return CreatedAtAction(nameof(GetById), new { id = profile.Id }, profile);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateSparkProfile request)
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        var profile = await _profileService.UpdateAsync(_userContext.UserId!.Value, request);
        if (profile == null)
            return NotFound();

        _logger.LogInformation("Updated Spark profile {ProfileId}", profile.Id);
        return Ok(profile);
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMyProfile()
    {
        if (!_userContext.IsAuthenticated)
            return Unauthorized();

        var deleted = await _profileService.DeleteAsync(_userContext.UserId!.Value);
        if (!deleted)
            return NotFound();

        _logger.LogInformation("Deleted Spark profile {ProfileId}", _userContext.UserId);
        return NoContent();
    }
}
