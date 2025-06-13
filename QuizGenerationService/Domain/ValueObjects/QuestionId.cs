namespace QuizGenerationService.Domain.ValueObjects;

public class QuestionId
{
    public string Value { get; }

    private QuestionId(string value)
    {
        Value = value;
    }

    public static QuestionId New() => new(Guid.NewGuid().ToString());

    public static QuestionId FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("QuestionId cannot be empty", nameof(value));
        
        return new QuestionId(value);
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        return obj is QuestionId other && Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();
} 