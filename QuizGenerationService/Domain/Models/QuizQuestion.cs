using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Domain.Models;

public class QuizQuestion
{
    public QuestionId Id { get; set; } = QuestionId.New();
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectAnswerIndex { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    
    public int? UserAnswerIndex { get; set; }
    public bool? IsCorrect { get; set; }
    public InterestRating? UserInterestRating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AnsweredAt { get; set; }

    public void RecordAnswer(int answerIndex)
    {
        UserAnswerIndex = answerIndex;
        IsCorrect = answerIndex == CorrectAnswerIndex;
        AnsweredAt = DateTime.UtcNow;
    }

    public void RecordInterest(int rating)
    {
        UserInterestRating = InterestRating.Create(rating);
    }

    public bool HasBeenAnswered => UserAnswerIndex.HasValue;
    public bool WasAnsweredCorrectly => IsCorrect == true;
} 