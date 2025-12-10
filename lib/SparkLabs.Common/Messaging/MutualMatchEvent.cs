using SparkLabs.Common.Models;

namespace SparkLabs.Common.Messaging;

public class MutualMatchEvent
{
    public Guid EventId { get; set; }
    public Guid CausingUserId { get; set; }
    public Guid AffectedUserId { get; set; }
    public Brand BrandId { get; set; }
    public DateTime Timestamp { get; set; }
}
