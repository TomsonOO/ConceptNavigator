namespace QuizGenerationService.Application.DTOs;

public class ExplainMoreRequestDto
{
    public string QuestionId { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public string? QuestionText { get; set; }
    public string? OriginalExplanation { get; set; }
    public string? Topic { get; set; }
    public string? Book { get; set; }
    public string Difficulty { get; set; } = "Medium";
    public string Language { get; set; } = "Polish";
    public List<string> UserInterests { get; set; } = new();
    public string? FocusArea { get; set; }
} 