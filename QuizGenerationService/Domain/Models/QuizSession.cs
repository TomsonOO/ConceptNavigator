using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Domain.Models;

public class QuizSession
{
    public SessionId Id { get; set; } = SessionId.New();
    public string Topic { get; set; } = string.Empty;
    public string? Book { get; set; }
    public QuestionType QuestionType { get; set; } = QuestionType.Basic;
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
    public Language Language { get; set; } = Language.Polish;
    public List<QuizQuestion> Questions { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
    public string? SessionName { get; set; }
    
    public void AddQuestion(QuizQuestion question)
    {
        Questions.Add(question);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddQuestions(IEnumerable<QuizQuestion> questions)
    {
        Questions.AddRange(questions);
        LastModifiedAt = DateTime.UtcNow;
    }

    public QuizQuestion? GetQuestion(QuestionId questionId)
    {
        return Questions.FirstOrDefault(q => q.Id.Equals(questionId));
    }

    public void RecordAnswer(QuestionId questionId, int answerIndex)
    {
        var question = GetQuestion(questionId);
        if (question == null)
            throw new InvalidOperationException($"Question with ID {questionId} not found in session");
        
        question.RecordAnswer(answerIndex);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void RecordInterest(QuestionId questionId, int rating)
    {
        var question = GetQuestion(questionId);
        if (question == null)
            throw new InvalidOperationException($"Question with ID {questionId} not found in session");
        
        question.RecordInterest(rating);
        LastModifiedAt = DateTime.UtcNow;
    }

    public List<QuizQuestion> GetHighInterestQuestions()
    {
        return Questions.Where(q => q.UserInterestRating?.IsHigh == true).ToList();
    }

    public List<string> GetHighInterestKeywords()
    {
        return GetHighInterestQuestions()
            .SelectMany(q => q.Keywords)
            .Distinct()
            .ToList();
    }

    public double GetAverageScore()
    {
        var answeredQuestions = Questions.Where(q => q.HasBeenAnswered).ToList();
        if (!answeredQuestions.Any())
            return 0;
        
        return answeredQuestions.Count(q => q.WasAnsweredCorrectly) / (double)answeredQuestions.Count;
    }

    public double GetAverageInterestRating()
    {
        var ratedQuestions = Questions.Where(q => q.UserInterestRating != null).ToList();
        if (!ratedQuestions.Any())
            return 0;
        
        return ratedQuestions.Average(q => q.UserInterestRating!.Value);
    }

    public int TotalQuestions => Questions.Count;
    public int AnsweredQuestions => Questions.Count(q => q.HasBeenAnswered);
    public int CorrectAnswers => Questions.Count(q => q.WasAnsweredCorrectly);
    public bool IsCompleted => Questions.Any() && Questions.All(q => q.HasBeenAnswered);
} 