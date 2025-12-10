using SparkLabs.Common.Models;

namespace SparkLabs.Common.Brands;

public abstract class BrandExtensionBase
{
    public Guid UserId { get; set; }
    public Brand BrandId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
