using Dapper;
using SparkLabs.Common.Models;

namespace SparkLabs.Common.Data;

public interface IInteractionRepository
{
    Task<UserInteractionEvent?> GetByIdAsync(Guid eventId);
    Task CreateAsync(UserInteractionEvent interaction);
    Task CreateBatchAsync(IEnumerable<UserInteractionEvent> interactions);
    Task<bool> HasLikedAsync(Guid fromUserId, Guid toUserId);
    Task<IEnumerable<UserInteractionEvent>> GetByUserAsync(Guid userId, int limit = 100, int offset = 0);
    Task<IEnumerable<UserInteractionEvent>> GetMatchesAsync(Guid userId);
    Task<IEnumerable<(Guid UserId, int ActivityCount)>> GetTopActiveUsersAsync(DateOnly date, int limit = 3);
}

public class InteractionRepository : IInteractionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public InteractionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<UserInteractionEvent?> GetByIdAsync(Guid eventId)
    {
        const string sql = """
            SELECT event_id AS EventId, from_user_id AS FromUserId, to_user_id AS ToUserId,
                   interaction_type AS Type, brand_id AS BrandId, timestamp AS Timestamp
            FROM user_interactions
            WHERE event_id = @EventId
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<UserInteractionEvent>(sql, new { EventId = eventId });
    }

    public async Task CreateAsync(UserInteractionEvent interaction)
    {
        const string sql = """
            INSERT INTO user_interactions (event_id, from_user_id, to_user_id, interaction_type, brand_id, timestamp)
            VALUES (@EventId, @FromUserId, @ToUserId, @Type, @BrandId, @Timestamp)
            """;

        if (interaction.EventId == Guid.Empty)
        {
            interaction.EventId = UuidV7.NewGuid();
        }

        if (interaction.Timestamp == default)
        {
            interaction.Timestamp = DateTime.UtcNow;
        }

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            interaction.EventId,
            interaction.FromUserId,
            interaction.ToUserId,
            Type = (int)interaction.Type,
            BrandId = (int)interaction.BrandId,
            interaction.Timestamp
        });
    }

    public async Task CreateBatchAsync(IEnumerable<UserInteractionEvent> interactions)
    {
        const string sql = """
            INSERT INTO user_interactions (event_id, from_user_id, to_user_id, interaction_type, brand_id, timestamp)
            VALUES (@EventId, @FromUserId, @ToUserId, @Type, @BrandId, @Timestamp)
            """;

        var records = interactions.Select(i => new
        {
            EventId = i.EventId == Guid.Empty ? UuidV7.NewGuid() : i.EventId,
            i.FromUserId,
            i.ToUserId,
            Type = (int)i.Type,
            BrandId = (int)i.BrandId,
            Timestamp = i.Timestamp == default ? DateTime.UtcNow : i.Timestamp
        });

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, records);
    }

    public async Task<bool> HasLikedAsync(Guid fromUserId, Guid toUserId)
    {
        const string sql = """
            SELECT EXISTS(
                SELECT 1 FROM user_interactions
                WHERE from_user_id = @FromUserId
                  AND to_user_id = @ToUserId
                  AND interaction_type = 1
            )
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(sql, new { FromUserId = fromUserId, ToUserId = toUserId });
    }

    public async Task<IEnumerable<UserInteractionEvent>> GetByUserAsync(Guid userId, int limit = 100, int offset = 0)
    {
        const string sql = """
            SELECT event_id AS EventId, from_user_id AS FromUserId, to_user_id AS ToUserId,
                   interaction_type AS Type, brand_id AS BrandId, timestamp AS Timestamp
            FROM user_interactions
            WHERE from_user_id = @UserId
            ORDER BY timestamp DESC
            LIMIT @Limit OFFSET @Offset
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<UserInteractionEvent>(sql, new { UserId = userId, Limit = limit, Offset = offset });
    }

    public async Task<IEnumerable<UserInteractionEvent>> GetMatchesAsync(Guid userId)
    {
        const string sql = """
            SELECT event_id AS EventId, from_user_id AS FromUserId, to_user_id AS ToUserId,
                   interaction_type AS Type, brand_id AS BrandId, timestamp AS Timestamp
            FROM user_interactions
            WHERE from_user_id = @UserId AND interaction_type = 3
            ORDER BY timestamp DESC
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<UserInteractionEvent>(sql, new { UserId = userId });
    }

    /// <summary>
    /// Gets the top N most active users for a given day.
    /// This is the query from the interview coding exercise.
    /// </summary>
    public async Task<IEnumerable<(Guid UserId, int ActivityCount)>> GetTopActiveUsersAsync(DateOnly date, int limit = 3)
    {
        const string sql = """
            SELECT from_user_id AS UserId, COUNT(*) AS ActivityCount
            FROM user_interactions
            WHERE timestamp >= @StartOfDay AND timestamp < @EndOfDay
            GROUP BY from_user_id
            ORDER BY ActivityCount DESC
            LIMIT @Limit
            """;

        var startOfDay = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endOfDay = date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        using var connection = _connectionFactory.CreateConnection();
        var results = await connection.QueryAsync<(Guid UserId, int ActivityCount)>(sql, new
        {
            StartOfDay = startOfDay,
            EndOfDay = endOfDay,
            Limit = limit
        });

        return results;
    }
}
