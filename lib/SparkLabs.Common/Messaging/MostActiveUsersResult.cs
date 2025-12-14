namespace SparkLabs.Common.Messaging;

public class MostActiveUsersResult
{
    public Guid CorrelationId { get; set; }
    public int BrandId { get; set; }
    public DateTime Timestamp { get; set; }
    public DateOnly Date { get; set; }
    public List<ActiveUserEntry> Users { get; set; } = new();
}

public class ActiveUserEntry
{
    public Guid UserId { get; set; }
    public int ActivityCount { get; set; }
}
