using System.Text.Json.Serialization;

namespace QuizGenerationService.Application.DTOs;

public class GenerateMoreQuestionsRequestDto
{
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;
    
    [JsonPropertyName("count")]
    public int QuestionCount { get; set; } = 3;
    
    [JsonPropertyName("questionType")]
    public string? QuestionType { get; set; }
    
    [JsonPropertyName("difficulty")]
    public string? Difficulty { get; set; }
    
    [JsonPropertyName("language")]
    public string? Language { get; set; }
    
    [JsonPropertyName("focusArea")]
    public string? FocusArea { get; set; }
    
    [JsonPropertyName("additionalInterests")]
    public List<string> AdditionalInterests { get; set; } = new();
    
    [JsonPropertyName("minInterestThreshold")]
    public double MinInterestThreshold { get; set; } = 0.6;
    
    [JsonPropertyName("includeRelatedConcepts")]
    public bool IncludeRelatedConcepts { get; set; } = true;
    
    [JsonPropertyName("avoidRepeatedTopics")]
    public bool AvoidRepeatedTopics { get; set; } = true;
} 