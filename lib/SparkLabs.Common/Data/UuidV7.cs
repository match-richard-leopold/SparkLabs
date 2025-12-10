namespace SparkLabs.Common.Data;

public static class UuidV7
{
    /// <summary>
    /// Generates a UUID v7 (time-sortable) using .NET 9's built-in support.
    /// </summary>
    public static Guid NewGuid() => Guid.CreateVersion7();
}
