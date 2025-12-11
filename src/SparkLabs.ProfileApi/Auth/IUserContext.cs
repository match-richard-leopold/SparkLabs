namespace SparkLabs.ProfileApi.Auth;

/// <summary>
/// Provides access to the current authenticated user's identity.
/// In production, this would be populated from JWT claims.
/// </summary>
public interface IUserContext
{
    Guid? UserId { get; set; }
    bool IsAuthenticated => UserId.HasValue;
}

public class UserContext : IUserContext
{
    public Guid? UserId { get; set; }
}
