using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Domain.Models;

public class ConceptNode
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ConceptType Type { get; set; } = ConceptType.General;
    public double ImportanceScore { get; set; } = 0.5;
    public string? RelatedPhilosopher { get; set; }
    public string? HistoricalPeriod { get; set; }
    public List<string> Keywords { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static ConceptNode Create(string name, string description, ConceptType type, double importanceScore = 0.5)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Concept name cannot be empty", nameof(name));
        
        if (importanceScore < 0.0 || importanceScore > 1.0)
            throw new ArgumentException("Importance score must be between 0.0 and 1.0", nameof(importanceScore));

        return new ConceptNode
        {
            Name = name,
            Description = description,
            Type = type,
            ImportanceScore = importanceScore
        };
    }

    public void AddKeyword(string keyword)
    {
        if (!string.IsNullOrWhiteSpace(keyword) && !Keywords.Contains(keyword))
        {
            Keywords.Add(keyword);
        }
    }

    public void SetPhilosopher(string philosopher)
    {
        if (!string.IsNullOrWhiteSpace(philosopher))
        {
            RelatedPhilosopher = philosopher;
        }
    }

    public void SetHistoricalPeriod(string period)
    {
        if (!string.IsNullOrWhiteSpace(period))
        {
            HistoricalPeriod = period;
        }
    }

    public bool IsHighImportance => ImportanceScore >= 0.8;
    public bool IsMediumImportance => ImportanceScore >= 0.4 && ImportanceScore < 0.8;
    public bool IsLowImportance => ImportanceScore < 0.4;

    public bool IsCorePhilosophicalConcept => Type == ConceptType.CoreConcept || Type == ConceptType.PhilosophicalProblem;
} 