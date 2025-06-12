namespace QuizGenerationService.Application.DTOs;

public class SaveSessionRequestDto
{
    public string? SessionName { get; set; }
    public QuizSessionDto Session { get; set; } = new();
} 