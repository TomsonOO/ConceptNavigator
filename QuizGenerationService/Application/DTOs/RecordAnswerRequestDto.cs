namespace QuizGenerationService.Application.DTOs;

public class RecordAnswerRequestDto
{
    public string SessionId { get; set; } = string.Empty;
    public string QuestionId { get; set; } = string.Empty;
    public int AnswerIndex { get; set; }
} 