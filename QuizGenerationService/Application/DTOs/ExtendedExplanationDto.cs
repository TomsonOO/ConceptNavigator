namespace QuizGenerationService.Application.DTOs;

public class ExtendedExplanationDto
{
    public string QuestionId { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string MainExplanation { get; set; } = string.Empty;
    public List<string> DetailedParagraphs { get; set; } = new();
    public string HistoricalContext { get; set; } = string.Empty;
    public string ContemporaryRelevance { get; set; } = string.Empty;
    public List<string> KeyConcepts { get; set; } = new();
    public List<string> RelatedPhilosophers { get; set; } = new();
    public List<SuggestedSourceDto> SuggestedSources { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string OriginalDifficulty { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public int TotalParagraphs { get; set; }
    public int EstimatedReadingTimeMinutes { get; set; }
} 