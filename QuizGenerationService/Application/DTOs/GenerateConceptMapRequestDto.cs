namespace QuizGenerationService.Application.DTOs;

public class GenerateConceptMapRequestDto
{
    public string? SessionId { get; set; }
    public string? Topic { get; set; }
    public string? Book { get; set; }
    public string? Language { get; set; }
    public List<string> FocusKeywords { get; set; } = new();
    public int MaxNodes { get; set; } = 15;
    public bool IncludePhilosophers { get; set; } = true;
    public bool IncludeHistoricalContext { get; set; } = true;
    public bool UseSessionInterests { get; set; } = true;
    public double MinImportanceThreshold { get; set; } = 0.3;
} 