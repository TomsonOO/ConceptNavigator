namespace QuizGenerationService.Domain.ValueObjects;

public class RelationshipType
{
    public string Value { get; }
    public string Description { get; }

    private RelationshipType(string value, string description)
    {
        Value = value;
        Description = description;
    }

    public static readonly RelationshipType Related = new("Related", "General relationship or connection");
    public static readonly RelationshipType PartOf = new("PartOf", "One concept is part of another");
    public static readonly RelationshipType InstanceOf = new("InstanceOf", "One concept is an instance of another");
    public static readonly RelationshipType Similar = new("Similar", "Concepts are similar or analogous");
    public static readonly RelationshipType Opposite = new("Opposite", "Concepts are opposing or contradictory");
    public static readonly RelationshipType Influences = new("Influences", "One concept influences another");
    public static readonly RelationshipType DevelopedBy = new("DevelopedBy", "Concept was developed by a philosopher");
    public static readonly RelationshipType Precedes = new("Precedes", "One concept historically precedes another");

    public static RelationshipType FromString(string value)
    {
        return value?.ToLower() switch
        {
            "related" => Related,
            "partof" => PartOf,
            "instanceof" => InstanceOf,
            "similar" => Similar,
            "opposite" => Opposite,
            "influences" => Influences,
            "developedby" => DevelopedBy,
            "precedes" => Precedes,
            _ => throw new ArgumentException($"Invalid relationship type: {value}")
        };
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        return obj is RelationshipType other && Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();
} 