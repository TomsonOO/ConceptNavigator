namespace QuizGenerationService.Domain.ValueObjects;

public class Language
{
    public string Code { get; }
    public string Name { get; }

    private Language(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public static Language Polish => new("pl", "Polish");
    public static Language English => new("en", "English");

    public static Language FromString(string value)
    {
        return value.ToLower() switch
        {
            "polish" or "pl" => Polish,
            "english" or "en" => English,
            _ => Polish
        };
    }

    public override string ToString() => Name;

    public override bool Equals(object? obj)
    {
        return obj is Language other && Code == other.Code;
    }

    public override int GetHashCode() => Code.GetHashCode();
} 