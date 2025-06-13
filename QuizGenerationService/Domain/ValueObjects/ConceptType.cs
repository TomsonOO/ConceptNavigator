namespace QuizGenerationService.Domain.ValueObjects;

public class ConceptType
{
    public string Value { get; }
    public string Description { get; }

    private ConceptType(string value, string description)
    {
        Value = value;
        Description = description;
    }

    public static readonly ConceptType CoreConcept = new("CoreConcept", "Central philosophical concept");
    public static readonly ConceptType PhilosophicalProblem = new("PhilosophicalProblem", "Major philosophical problem or question");
    public static readonly ConceptType Philosopher = new("Philosopher", "Individual philosopher or thinker");
    public static readonly ConceptType School = new("School", "Philosophical school or movement");
    public static readonly ConceptType HistoricalPeriod = new("HistoricalPeriod", "Historical period or era");
    public static readonly ConceptType Method = new("Method", "Philosophical method or approach");
    public static readonly ConceptType General = new("General", "General concept or term");

    public static ConceptType FromString(string value)
    {
        return value?.ToLower() switch
        {
            "coreconcept" => CoreConcept,
            "philosophicalproblem" => PhilosophicalProblem,
            "philosopher" => Philosopher,
            "school" => School,
            "historicalperiod" => HistoricalPeriod,
            "method" => Method,
            "general" => General,
            _ => throw new ArgumentException($"Invalid concept type: {value}")
        };
    }

    public static ConceptType FromCategory(string category)
    {
        return category?.ToLower() switch
        {
            "concept" => CoreConcept,
            "philosopher" => Philosopher,
            "historical" => HistoricalPeriod,
            "technical" => Method,
            _ => General
        };
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        return obj is ConceptType other && Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();
} 