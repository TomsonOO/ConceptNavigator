using System.Text.Json.Serialization;

namespace QuizGenerationService.Application.DTOs;

public class SuggestedSourceDto
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("relevanceScore")]
    public int RelevanceScore { get; set; }
} 