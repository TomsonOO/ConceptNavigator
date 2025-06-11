using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Domain.Models;

public class Quiz
{
    public string Topic { get; set; } = string.Empty;
    public string? Book { get; set; }
    public QuestionType QuestionType { get; set; } = QuestionType.Basic;
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
    public Language Language { get; set; } = Language.Polish;
    public List<QuizQuestion> Questions { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
} 