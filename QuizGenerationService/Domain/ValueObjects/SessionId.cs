namespace QuizGenerationService.Domain.ValueObjects;

public class SessionId
{
    public string Value { get; }

    private SessionId(string value)
    {
        Value = value;
    }

    public static SessionId New() => new(Guid.NewGuid().ToString());

    public static SessionId FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SessionId cannot be empty", nameof(value));
        
        return new SessionId(value);
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        return obj is SessionId other && Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();
} 