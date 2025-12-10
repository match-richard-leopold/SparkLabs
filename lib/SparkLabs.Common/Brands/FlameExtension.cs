namespace SparkLabs.Common.Brands;

public class FlameExtension : BrandExtensionBase
{
    public RelationshipGoal RelationshipGoal { get; set; }
    public FamilyPlans FamilyPlans { get; set; }
    public PoliticalLeaning PoliticalLeaning { get; set; }
    public ReligiousAffiliation Religion { get; set; }
    public bool WantsPets { get; set; }
    public DietaryPreference DietaryPreference { get; set; }
}

public enum RelationshipGoal
{
    Unknown = 0,
    Casual = 1,
    LongTerm = 2,
    Marriage = 3,
    Undecided = 4
}

public enum FamilyPlans
{
    Unknown = 0,
    WantsChildren = 1,
    DoesNotWantChildren = 2,
    HasChildren = 3,
    OpenToChildren = 4,
    Undecided = 5
}

public enum PoliticalLeaning
{
    Unknown = 0,
    Liberal = 1,
    Conservative = 2,
    Moderate = 3,
    Libertarian = 4,
    Apolitical = 5,
    Other = 6
}

public enum ReligiousAffiliation
{
    Unknown = 0,
    Christian = 1,
    Jewish = 2,
    Muslim = 3,
    Hindu = 4,
    Buddhist = 5,
    Spiritual = 6,
    Agnostic = 7,
    Atheist = 8,
    Other = 9
}

public enum DietaryPreference
{
    Unknown = 0,
    NoRestrictions = 1,
    Vegetarian = 2,
    Vegan = 3,
    Pescatarian = 4,
    Kosher = 5,
    Halal = 6,
    GlutenFree = 7,
    Other = 8
}
