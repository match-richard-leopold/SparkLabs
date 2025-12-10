namespace SparkLabs.Common.Models;

public class UserProfile
{
    public Guid Id { get; set; }
    public Brand BrandId { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public Gender Gender { get; set; }
    public Gender SeekGender { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public bool IsActive { get; set; }
}

public enum Gender
{
    Unknown = 0,
    Male = 1,
    Female = 2,
    NonBinary = 3,
    Other = 4
}
