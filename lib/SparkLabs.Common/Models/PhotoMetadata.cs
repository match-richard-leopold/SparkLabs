namespace SparkLabs.Common.Models;

public class PhotoMetadata
{
    /// <summary>
    /// Partition key: {brandId}#{userId}
    /// </summary>
    public required string Pk { get; set; }

    /// <summary>
    /// Sort key: {photoId}
    /// </summary>
    public required string Sk { get; set; }

    public required string S3Path { get; set; }

    public bool IsVisible { get; set; } = true;

    public DateTime UploadedAt { get; set; }

    // Convenience properties (derived from Pk/Sk)
    public string BrandId => Pk.Split('#')[0];
    public Guid UserId => Guid.Parse(Pk.Split('#')[1]);
    public Guid PhotoId => Guid.Parse(Sk);

    public static PhotoMetadata Create(string brandId, Guid userId, Guid photoId, string s3Path)
    {
        return new PhotoMetadata
        {
            Pk = $"{brandId}#{userId}",
            Sk = photoId.ToString(),
            S3Path = s3Path,
            IsVisible = true,
            UploadedAt = DateTime.UtcNow
        };
    }
}
