namespace QuizGenerationService.Application.DTOs;

public class SuggestedSourceDto
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int RelevanceScore { get; set; }
} 