using SparkLabs.Common.Brands;
using SparkLabs.Common.Data;
using SparkLabs.Common.Models;

namespace SparkLabs.Common.Services;

public interface IKindlingProfileService
{
    Task<KindlingProfile?> GetByIdAsync(Guid userId);
    Task<IEnumerable<KindlingProfile>> ListAsync(int limit, int offset);
    Task<KindlingProfile> CreateAsync(CreateKindlingProfile request);
    Task<KindlingProfile?> UpdateAsync(Guid userId, UpdateKindlingProfile request);
    Task<bool> DeleteAsync(Guid userId);
}

public class KindlingProfileService : IKindlingProfileService
{
    private readonly IProfileRepository _profileRepository;
    private readonly IKindlingExtensionRepository _extensionRepository;

    public KindlingProfileService(
        IProfileRepository profileRepository,
        IKindlingExtensionRepository extensionRepository)
    {
        _profileRepository = profileRepository;
        _extensionRepository = extensionRepository;
    }

    public async Task<KindlingProfile?> GetByIdAsync(Guid userId)
    {
        var profile = await _profileRepository.GetByIdAsync(userId);
        if (profile == null || profile.BrandId != Brand.Kindling)
            return null;

        var extension = await _extensionRepository.GetByUserIdAsync(userId);
        return KindlingProfile.From(profile, extension);
    }

    public async Task<IEnumerable<KindlingProfile>> ListAsync(int limit, int offset)
    {
        var profiles = await _profileRepository.GetByBrandAsync(Brand.Kindling, limit, offset);

        var results = new List<KindlingProfile>();
        foreach (var profile in profiles)
        {
            var extension = await _extensionRepository.GetByUserIdAsync(profile.Id);
            results.Add(KindlingProfile.From(profile, extension));
        }

        return results;
    }

    public async Task<KindlingProfile> CreateAsync(CreateKindlingProfile request)
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
            CompatibleSigns = request.CompatibleSigns
        };

        await _extensionRepository.SaveAsync(extension);

        return KindlingProfile.From(profile, extension);
    }

    public async Task<KindlingProfile?> UpdateAsync(Guid userId, UpdateKindlingProfile request)
    {
        var profile = await _profileRepository.GetByIdAsync(userId);
        if (profile == null || profile.BrandId != Brand.Kindling)
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
            ?? new KindlingExtension { UserId = userId, BrandId = Brand.Kindling };

        extension.SunSign = request.SunSign ?? extension.SunSign;
        extension.RisingSign = request.RisingSign ?? extension.RisingSign;
        extension.MoonSign = request.MoonSign ?? extension.MoonSign;
        extension.BelievesInAstrology = request.BelievesInAstrology ?? extension.BelievesInAstrology;
        extension.CompatibleSigns = request.CompatibleSigns ?? extension.CompatibleSigns;

        await _extensionRepository.SaveAsync(extension);

        return KindlingProfile.From(profile, extension);
    }

    public async Task<bool> DeleteAsync(Guid userId)
    {
        var profile = await _profileRepository.GetByIdAsync(userId);
        if (profile == null || profile.BrandId != Brand.Kindling)
            return false;

        await _profileRepository.DeleteAsync(userId);
        await _extensionRepository.DeleteAsync(userId);

        return true;
    }
}

// Domain models for Kindling

public record CreateKindlingProfile(
    string Email,
    string DisplayName,
    DateTime DateOfBirth,
    string? Bio,
    string? Location,
    Gender Gender,
    Gender SeekGender,
    ZodiacSign SunSign,
    ZodiacSign? RisingSign,
    ZodiacSign? MoonSign,
    bool BelievesInAstrology,
    List<ZodiacSign> CompatibleSigns
);

public record UpdateKindlingProfile(
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

public record KindlingProfile(
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
    ZodiacSign SunSign,
    ZodiacSign? RisingSign,
    ZodiacSign? MoonSign,
    bool BelievesInAstrology,
    List<ZodiacSign> CompatibleSigns
)
{
    public static KindlingProfile From(UserProfile profile, KindlingExtension? extension)
    {
        return new KindlingProfile(
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
