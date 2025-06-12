namespace QuizGenerationService.Domain.ValueObjects;

public class InterestRating
{
    public int Value { get; }

    private InterestRating(int value)
    {
        Value = value;
    }

    public static InterestRating Create(int value)
    {
        if (value < 1 || value > 10)
            throw new ArgumentException("Interest rating must be between 1 and 10", nameof(value));
        
        return new InterestRating(value);
    }

    public static InterestRating? FromNullableInt(int? value)
    {
        return value.HasValue ? Create(value.Value) : null;
    }

    public bool IsHigh => Value >= 8;
    public bool IsMedium => Value >= 5 && Value <= 7;
    public bool IsLow => Value <= 4;

    public override string ToString() => Value.ToString();

    public override bool Equals(object? obj)
    {
        return obj is InterestRating other && Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();
} 