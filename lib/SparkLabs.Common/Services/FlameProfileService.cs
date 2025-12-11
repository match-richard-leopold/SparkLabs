using SparkLabs.Common.Brands;
using SparkLabs.Common.Data;
using SparkLabs.Common.Models;

namespace SparkLabs.Common.Services;

public interface IFlameProfileService
{
    Task<FlameProfile?> GetByIdAsync(Guid userId);
    Task<IEnumerable<FlameProfile>> ListAsync(int limit, int offset);
    Task<FlameProfile> CreateAsync(CreateFlameProfile request);
    Task<FlameProfile?> UpdateAsync(Guid userId, UpdateFlameProfile request);
    Task<bool> DeleteAsync(Guid userId);
}

public class FlameProfileService : IFlameProfileService
{
    private readonly IProfileRepository _profileRepository;
    private readonly IFlameExtensionRepository _extensionRepository;

    public FlameProfileService(
        IProfileRepository profileRepository,
        IFlameExtensionRepository extensionRepository)
    {
        _profileRepository = profileRepository;
        _extensionRepository = extensionRepository;
    }

    public async Task<FlameProfile?> GetByIdAsync(Guid userId)
    {
        var profile = await _profileRepository.GetByIdAsync(userId);
        if (profile == null || profile.BrandId != Brand.Flame)
            return null;

        var extension = await _extensionRepository.GetByUserIdAsync(userId);
        return FlameProfile.From(profile, extension);
    }

    public async Task<IEnumerable<FlameProfile>> ListAsync(int limit, int offset)
    {
        var profiles = await _profileRepository.GetByBrandAsync(Brand.Flame, limit, offset);

        var results = new List<FlameProfile>();
        foreach (var profile in profiles)
        {
            var extension = await _extensionRepository.GetByUserIdAsync(profile.Id);
            results.Add(FlameProfile.From(profile, extension));
        }

        return results;
    }

    public async Task<FlameProfile> CreateAsync(CreateFlameProfile request)
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

        return FlameProfile.From(profile, extension);
    }

    public async Task<FlameProfile?> UpdateAsync(Guid userId, UpdateFlameProfile request)
    {
        var profile = await _profileRepository.GetByIdAsync(userId);
        if (profile == null || profile.BrandId != Brand.Flame)
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
            ?? new FlameExtension { UserId = userId, BrandId = Brand.Flame };

        extension.RelationshipGoal = request.RelationshipGoal ?? extension.RelationshipGoal;
        extension.FamilyPlans = request.FamilyPlans ?? extension.FamilyPlans;
        extension.PoliticalLeaning = request.PoliticalLeaning ?? extension.PoliticalLeaning;
        extension.Religion = request.Religion ?? extension.Religion;
        extension.WantsPets = request.WantsPets ?? extension.WantsPets;
        extension.DietaryPreference = request.DietaryPreference ?? extension.DietaryPreference;

        await _extensionRepository.SaveAsync(extension);

        return FlameProfile.From(profile, extension);
    }

    public async Task<bool> DeleteAsync(Guid userId)
    {
        var profile = await _profileRepository.GetByIdAsync(userId);
        if (profile == null || profile.BrandId != Brand.Flame)
            return false;

        await _profileRepository.DeleteAsync(userId);
        await _extensionRepository.DeleteAsync(userId);

        return true;
    }
}

// Domain models for Flame

public record CreateFlameProfile(
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

public record UpdateFlameProfile(
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

public record FlameProfile(
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
)
{
    public static FlameProfile From(UserProfile profile, FlameExtension? extension)
    {
        return new FlameProfile(
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
