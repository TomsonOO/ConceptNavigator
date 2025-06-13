using System.Text.Json.Serialization;

namespace QuizGenerationService.Application.DTOs;

public class ExplainMoreRequestDto
{
    [JsonPropertyName("questionId")]
    public string QuestionId { get; set; } = string.Empty;
    
    [JsonPropertyName("sessionId")]
    public string? SessionId { get; set; }
    
    [JsonPropertyName("questionText")]
    public string? QuestionText { get; set; }
    
    [JsonPropertyName("originalExplanation")]
    public string? OriginalExplanation { get; set; }
    
    [JsonPropertyName("topic")]
    public string? Topic { get; set; }
    
    [JsonPropertyName("book")]
    public string? Book { get; set; }
    
    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = "Medium";
    
    [JsonPropertyName("language")]
    public string Language { get; set; } = "Polish";
    
    [JsonPropertyName("userInterests")]
    public List<string> UserInterests { get; set; } = new();
    
    [JsonPropertyName("focusArea")]
    public string? FocusArea { get; set; }
} 