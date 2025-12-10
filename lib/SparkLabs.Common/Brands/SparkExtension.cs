namespace SparkLabs.Common.Brands;

public class SparkExtension : BrandExtensionBase
{
    public List<string> Hobbies { get; set; } = [];
    public ActivityLevel ActivityLevel { get; set; }
    public WeekendStyle WeekendStyle { get; set; }
    public List<string> FavoriteActivities { get; set; } = [];
    public bool OpenToNewHobbies { get; set; }
}

public enum ActivityLevel
{
    Unknown = 0,
    Sedentary = 1,
    LightlyActive = 2,
    ModeratelyActive = 3,
    VeryActive = 4,
    ExtremelyActive = 5
}

public enum WeekendStyle
{
    Unknown = 0,
    Homebody = 1,
    Adventurer = 2,
    Social = 3,
    Mixed = 4
}
