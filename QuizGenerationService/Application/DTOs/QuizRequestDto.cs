namespace QuizGenerationService.Application.DTOs;

public class QuizRequestDto
{
    public string Topic { get; set; } = string.Empty;
    public string? Book { get; set; }
    public string QuestionType { get; set; } = "basic";
    public string Difficulty { get; set; } = "medium";
    public string Language { get; set; } = "polish";
    public int QuestionCount { get; set; } = 5;
} 