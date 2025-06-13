namespace QuizGenerationService.Domain.ValueObjects;

public class QuestionType
{
    public string Value { get; }

    private QuestionType(string value)
    {
        Value = value;
    }

    public static QuestionType Basic => new("Basic");
    public static QuestionType Situational => new("Situational");

    public static QuestionType FromString(string value)
    {
        return value.ToLower() switch
        {
            "basic" => Basic,
            "situational" => Situational,
            _ => Basic
        };
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        return obj is QuestionType other && Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();
} 