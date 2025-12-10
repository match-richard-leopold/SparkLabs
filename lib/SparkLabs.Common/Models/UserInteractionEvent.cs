using SparkLabs.Common.Models;

namespace SparkLabs.Common.Models;

public class UserInteractionEvent
{
    public Guid EventId { get; set; }
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public InteractionType Type { get; set; }
    public Brand BrandId { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum InteractionType
{
    Like = 1,
    Pass = 2,
    MutualMatch = 3
}
