namespace QuizGenerationService.Application.DTOs;

public class RecordInterestRequestDto
{
    public string SessionId { get; set; } = string.Empty;
    public string QuestionId { get; set; } = string.Empty;
    public int InterestRating { get; set; }
} 