using System.Text.Json.Serialization;

namespace QuizGenerationService.Application.DTOs;

public class ExtendedExplanationDto
{
    [JsonPropertyName("questionId")]
    public string QuestionId { get; set; } = string.Empty;
    
    [JsonPropertyName("questionText")]
    public string QuestionText { get; set; } = string.Empty;
    
    [JsonPropertyName("mainExplanation")]
    public string MainExplanation { get; set; } = string.Empty;
    
    [JsonPropertyName("detailedParagraphs")]
    public List<string> DetailedParagraphs { get; set; } = new();
    
    [JsonPropertyName("historicalContext")]
    public string HistoricalContext { get; set; } = string.Empty;
    
    [JsonPropertyName("contemporaryRelevance")]
    public string ContemporaryRelevance { get; set; } = string.Empty;
    
    [JsonPropertyName("keyConcepts")]
    public List<string> KeyConcepts { get; set; } = new();
    
    [JsonPropertyName("relatedPhilosophers")]
    public List<string> RelatedPhilosophers { get; set; } = new();
    
    [JsonPropertyName("suggestedSources")]
    public List<SuggestedSourceDto> SuggestedSources { get; set; } = new();
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("originalDifficulty")]
    public string OriginalDifficulty { get; set; } = string.Empty;
    
    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;
    
    [JsonPropertyName("totalParagraphs")]
    public int TotalParagraphs { get; set; }
    
    [JsonPropertyName("estimatedReadingTimeMinutes")]
    public int EstimatedReadingTimeMinutes { get; set; }
} 