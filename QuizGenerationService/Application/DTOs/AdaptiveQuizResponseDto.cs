using System.Text.Json.Serialization;

namespace QuizGenerationService.Application.DTOs;

public class AdaptiveQuizResponseDto
{
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;
    
    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;
    
    [JsonPropertyName("book")]
    public string? Book { get; set; }
    
    [JsonPropertyName("questionType")]
    public string QuestionType { get; set; } = string.Empty;
    
    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = string.Empty;
    
    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;
    
    [JsonPropertyName("questions")]
    public List<QuizQuestionDto> Questions { get; set; } = new();
    
    [JsonPropertyName("totalQuestionsInSession")]
    public int TotalQuestionsInSession { get; set; }
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("keywordsUsed")]
    public List<string> KeywordsUsed { get; set; } = new();
    
    [JsonPropertyName("userInterestsApplied")]
    public List<string> UserInterestsApplied { get; set; } = new();
    
    [JsonPropertyName("focusArea")]
    public string? FocusArea { get; set; }
    
    [JsonPropertyName("adaptationScore")]
    public double AdaptationScore { get; set; }
    
    [JsonPropertyName("wasPersonalized")]
    public bool WasPersonalized { get; set; }
    
    [JsonPropertyName("personalizationReason")]
    public string PersonalizationReason { get; set; } = string.Empty;
} 