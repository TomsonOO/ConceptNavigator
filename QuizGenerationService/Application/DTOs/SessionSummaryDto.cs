namespace QuizGenerationService.Application.DTOs;

public class SessionSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string? SessionName { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string? Book { get; set; }
    public string QuestionType { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public int TotalQuestions { get; set; }
    public int AnsweredQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public bool IsCompleted { get; set; }
    public double AverageScore { get; set; }
    public double AverageInterestRating { get; set; }
} 