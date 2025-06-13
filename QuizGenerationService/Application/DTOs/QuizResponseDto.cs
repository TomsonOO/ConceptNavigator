namespace QuizGenerationService.Application.DTOs;

public class QuizResponseDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string? Book { get; set; }
    public string QuestionType { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public List<QuizQuestionDto> Questions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
} 