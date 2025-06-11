namespace QuizGenerationService.Domain.ValueObjects;

public class DifficultyLevel
{
    public string Value { get; }
    public string Description { get; }

    private DifficultyLevel(string value, string description)
    {
        Value = value;
        Description = description;
    }

    public static DifficultyLevel Easy => new("Easy", "Simple definitions and obvious applications");
    public static DifficultyLevel Medium => new("Medium", "Nuanced concepts and moderate scenarios");
    public static DifficultyLevel Hard => new("Hard", "Abstract thinking and complex dilemmas");

    public static DifficultyLevel FromString(string value)
    {
        return value.ToLower() switch
        {
            "easy" => Easy,
            "medium" => Medium,
            "hard" => Hard,
            _ => Medium
        };
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        return obj is DifficultyLevel other && Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();
} 