using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Domain.Models;

public class SessionSummary
{
    public SessionId Id { get; set; } = SessionId.New();
    public string? SessionName { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string? Book { get; set; }
    public QuestionType QuestionType { get; set; } = QuestionType.Basic;
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
    public Language Language { get; set; } = Language.Polish;
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public int TotalQuestions { get; set; }
    public int AnsweredQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public bool IsCompleted { get; set; }
    public double AverageScore { get; set; }
    public double AverageInterestRating { get; set; }

    public static SessionSummary FromSession(QuizSession session)
    {
        return new SessionSummary
        {
            Id = session.Id,
            SessionName = session.SessionName,
            Topic = session.Topic,
            Book = session.Book,
            QuestionType = session.QuestionType,
            Difficulty = session.Difficulty,
            Language = session.Language,
            CreatedAt = session.CreatedAt,
            LastModifiedAt = session.LastModifiedAt,
            TotalQuestions = session.TotalQuestions,
            AnsweredQuestions = session.AnsweredQuestions,
            CorrectAnswers = session.CorrectAnswers,
            IsCompleted = session.IsCompleted,
            AverageScore = session.GetAverageScore(),
            AverageInterestRating = session.GetAverageInterestRating()
        };
    }
} 