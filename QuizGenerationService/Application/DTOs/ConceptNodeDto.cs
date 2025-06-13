namespace QuizGenerationService.Application.DTOs;

public class ConceptNodeDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double ImportanceScore { get; set; }
    public string? RelatedPhilosopher { get; set; }
    public string? HistoricalPeriod { get; set; }
    public List<string> Keywords { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool IsHighImportance { get; set; }
    public bool IsCorePhilosophicalConcept { get; set; }
    public int ConnectivityCount { get; set; }
} 