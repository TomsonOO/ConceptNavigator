namespace QuizGenerationService.Application.DTOs;

public class QuizQuestionDto
{
    public string Id { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectAnswerIndex { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    
    public int? UserAnswerIndex { get; set; }
    public bool? IsCorrect { get; set; }
    public int? UserInterestRating { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AnsweredAt { get; set; }
} 