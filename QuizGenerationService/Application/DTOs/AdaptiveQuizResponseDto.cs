namespace QuizGenerationService.Application.DTOs;

public class AdaptiveQuizResponseDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string? Book { get; set; }
    public string QuestionType { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public List<QuizQuestionDto> Questions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    
    public List<string> KeywordsUsed { get; set; } = new();
    public List<string> UserInterestsApplied { get; set; } = new();
    public string? FocusArea { get; set; }
    public double AdaptationScore { get; set; }
    public bool WasPersonalized { get; set; }
    public string PersonalizationReason { get; set; } = string.Empty;
} 