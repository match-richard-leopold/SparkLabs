using Dapper;
using SparkLabs.Common.Models;

namespace SparkLabs.Common.Data;

public interface IProfileRepository
{
    Task<UserProfile?> GetByIdAsync(Guid id);
    Task<UserProfile?> GetByEmailAsync(string email);
    Task<IEnumerable<UserProfile>> GetByBrandAsync(Brand brand, int limit = 100, int offset = 0);
    Task<Guid> CreateAsync(UserProfile profile);
    Task<bool> UpdateAsync(UserProfile profile);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> UpdateLastActiveAsync(Guid id);
}

public class ProfileRepository : IProfileRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProfileRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<UserProfile?> GetByIdAsync(Guid id)
    {
        const string sql = """
            SELECT id, brand_id AS BrandId, email, display_name AS DisplayName,
                   date_of_birth AS DateOfBirth, bio, location, gender,
                   seek_gender AS SeekGender, created_at AS CreatedAt,
                   updated_at AS UpdatedAt, last_active_at AS LastActiveAt, is_active AS IsActive
            FROM user_profiles
            WHERE id = @Id
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<UserProfile>(sql, new { Id = id });
    }

    public async Task<UserProfile?> GetByEmailAsync(string email)
    {
        const string sql = """
            SELECT id, brand_id AS BrandId, email, display_name AS DisplayName,
                   date_of_birth AS DateOfBirth, bio, location, gender,
                   seek_gender AS SeekGender, created_at AS CreatedAt,
                   updated_at AS UpdatedAt, last_active_at AS LastActiveAt, is_active AS IsActive
            FROM user_profiles
            WHERE email = @Email
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<UserProfile>(sql, new { Email = email });
    }

    public async Task<IEnumerable<UserProfile>> GetByBrandAsync(Brand brand, int limit = 100, int offset = 0)
    {
        const string sql = """
            SELECT id, brand_id AS BrandId, email, display_name AS DisplayName,
                   date_of_birth AS DateOfBirth, bio, location, gender,
                   seek_gender AS SeekGender, created_at AS CreatedAt,
                   updated_at AS UpdatedAt, last_active_at AS LastActiveAt, is_active AS IsActive
            FROM user_profiles
            WHERE brand_id = @BrandId AND is_active = TRUE
            ORDER BY created_at DESC
            LIMIT @Limit OFFSET @Offset
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<UserProfile>(sql, new { BrandId = (int)brand, Limit = limit, Offset = offset });
    }

    public async Task<Guid> CreateAsync(UserProfile profile)
    {
        const string sql = """
            INSERT INTO user_profiles (id, brand_id, email, display_name, date_of_birth, bio, location,
                                       gender, seek_gender, created_at, updated_at, last_active_at, is_active)
            VALUES (@Id, @BrandId, @Email, @DisplayName, @DateOfBirth, @Bio, @Location,
                    @Gender, @SeekGender, @CreatedAt, @UpdatedAt, @LastActiveAt, @IsActive)
            """;

        profile.Id = UuidV7.NewGuid();
        profile.CreatedAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;
        profile.IsActive = true;

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            profile.Id,
            BrandId = (int)profile.BrandId,
            profile.Email,
            profile.DisplayName,
            profile.DateOfBirth,
            profile.Bio,
            profile.Location,
            Gender = (int)profile.Gender,
            SeekGender = (int)profile.SeekGender,
            profile.CreatedAt,
            profile.UpdatedAt,
            profile.LastActiveAt,
            profile.IsActive
        });

        return profile.Id;
    }

    public async Task<bool> UpdateAsync(UserProfile profile)
    {
        const string sql = """
            UPDATE user_profiles
            SET display_name = @DisplayName, bio = @Bio, location = @Location,
                gender = @Gender, seek_gender = @SeekGender, updated_at = @UpdatedAt
            WHERE id = @Id
            """;

        profile.UpdatedAt = DateTime.UtcNow;

        using var connection = _connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(sql, new
        {
            profile.Id,
            profile.DisplayName,
            profile.Bio,
            profile.Location,
            Gender = (int)profile.Gender,
            SeekGender = (int)profile.SeekGender,
            profile.UpdatedAt
        });

        return affected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        const string sql = """
            UPDATE user_profiles
            SET is_active = FALSE, updated_at = @UpdatedAt
            WHERE id = @Id
            """;

        using var connection = _connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });

        return affected > 0;
    }

    public async Task<bool> UpdateLastActiveAsync(Guid id)
    {
        const string sql = """
            UPDATE user_profiles
            SET last_active_at = @LastActiveAt
            WHERE id = @Id
            """;

        using var connection = _connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(sql, new { Id = id, LastActiveAt = DateTime.UtcNow });

        return affected > 0;
    }
}
