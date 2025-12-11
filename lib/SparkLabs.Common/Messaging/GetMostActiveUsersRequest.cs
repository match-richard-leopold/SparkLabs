namespace SparkLabs.Common.Messaging;

public class GetMostActiveUsersRequest
{
    public Guid CorrelationId { get; set; }
    public int? BrandId { get; set; }
    public int Limit { get; set; } = 10;
}
