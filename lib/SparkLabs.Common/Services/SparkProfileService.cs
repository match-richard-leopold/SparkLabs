using SparkLabs.Common.Brands;
using SparkLabs.Common.Data;
using SparkLabs.Common.Models;

namespace SparkLabs.Common.Services;

public interface ISparkProfileService
{
    Task<SparkProfile?> GetByIdAsync(Guid userId);
    Task<IEnumerable<SparkProfile>> ListAsync(int limit, int offset);
    Task<SparkProfile> CreateAsync(CreateSparkProfile request);
    Task<SparkProfile?> UpdateAsync(Guid userId, UpdateSparkProfile request);
    Task<bool> DeleteAsync(Guid userId);
}

public class SparkProfileService : ISparkProfileService
{
    private readonly IProfileRepository _profileRepository;
    private readonly ISparkExtensionRepository _extensionRepository;

    public SparkProfileService(
        IProfileRepository profileRepository,
        ISparkExtensionRepository extensionRepository)
    {
        _profileRepository = profileRepository;
        _extensionRepository = extensionRepository;
    }

    public async Task<SparkProfile?> GetByIdAsync(Guid userId)
    {
        var profile = await _profileRepository.GetByIdAsync(userId);
        if (profile == null || profile.BrandId != Brand.Spark)
            return null;

        var extension = await _extensionRepository.GetByUserIdAsync(userId);
        return SparkProfile.From(profile, extension);
    }

    public async Task<IEnumerable<SparkProfile>> ListAsync(int limit, int offset)
    {
        var profiles = await _profileRepository.GetByBrandAsync(Brand.Spark, limit, offset);

        var results = new List<SparkProfile>();
        foreach (var profile in profiles)
        {
            var extension = await _extensionRepository.GetByUserIdAsync(profile.Id);
            results.Add(SparkProfile.From(profile, extension));
        }

        return results;
    }

    public async Task<SparkProfile> CreateAsync(CreateSparkProfile request)
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
            Hobbies = request.Hobbies,
            ActivityLevel = request.ActivityLevel,
            WeekendStyle = request.WeekendStyle,
            FavoriteActivities = request.FavoriteActivities,
            OpenToNewHobbies = request.OpenToNewHobbies
        };

        await _extensionRepository.SaveAsync(extension);

        return SparkProfile.From(profile, extension);
    }

    public async Task<SparkProfile?> UpdateAsync(Guid userId, UpdateSparkProfile request)
    {
        var profile = await _profileRepository.GetByIdAsync(userId);
        if (profile == null || profile.BrandId != Brand.Spark)
            return null;

        // Update core profile fields
        profile.DisplayName = request.DisplayName ?? profile.DisplayName;
        profile.Bio = request.Bio ?? profile.Bio;
        profile.Location = request.Location ?? profile.Location;
        profile.Gender = request.Gender ?? profile.Gender;
        profile.SeekGender = request.SeekGender ?? profile.SeekGender;

        await _profileRepository.UpdateAsync(profile);

        // Update extension
        var extension = await _extensionRepository.GetByUserIdAsync(userId)
            ?? new SparkExtension { UserId = userId, BrandId = Brand.Spark };

        extension.Hobbies = request.Hobbies ?? extension.Hobbies;
        extension.ActivityLevel = request.ActivityLevel ?? extension.ActivityLevel;
        extension.WeekendStyle = request.WeekendStyle ?? extension.WeekendStyle;
        extension.FavoriteActivities = request.FavoriteActivities ?? extension.FavoriteActivities;
        extension.OpenToNewHobbies = request.OpenToNewHobbies ?? extension.OpenToNewHobbies;

        await _extensionRepository.SaveAsync(extension);

        return SparkProfile.From(profile, extension);
    }

    public async Task<bool> DeleteAsync(Guid userId)
    {
        var profile = await _profileRepository.GetByIdAsync(userId);
        if (profile == null || profile.BrandId != Brand.Spark)
            return false;

        await _profileRepository.DeleteAsync(userId);
        await _extensionRepository.DeleteAsync(userId);

        return true;
    }
}

// Domain models for Spark

public record CreateSparkProfile(
    string Email,
    string DisplayName,
    DateTime DateOfBirth,
    string? Bio,
    string? Location,
    Gender Gender,
    Gender SeekGender,
    List<string> Hobbies,
    ActivityLevel ActivityLevel,
    WeekendStyle WeekendStyle,
    List<string> FavoriteActivities,
    bool OpenToNewHobbies
);

public record UpdateSparkProfile(
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

public record SparkProfile(
    Guid Id,
    string Email,
    string DisplayName,
    DateTime DateOfBirth,
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
)
{
    public static SparkProfile From(UserProfile profile, SparkExtension? extension)
    {
        return new SparkProfile(
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
