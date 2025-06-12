using QuizGenerationService.Application.DTOs;
using QuizGenerationService.Application.Interfaces;
using QuizGenerationService.Domain.Models;
using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Application.Mapping;

public class QuizMappingService : IQuizMappingService
{
    public QuizGenerationRequest MapToRequest(QuizRequestDto dto)
    {
        return new QuizGenerationRequest
        {
            Topic = dto.Topic,
            Book = dto.Book,
            QuestionType = QuestionType.FromString(dto.QuestionType),
            Difficulty = DifficultyLevel.FromString(dto.Difficulty),
            Language = Language.FromString(dto.Language),
            QuestionCount = dto.QuestionCount
        };
    }

    public QuizResponseDto MapToResponse(Quiz quiz)
    {
        return new QuizResponseDto
        {
            Topic = quiz.Topic,
            Book = quiz.Book,
            QuestionType = quiz.QuestionType.Value,
            Difficulty = quiz.Difficulty.Value,
            Language = quiz.Language.Name,
            Questions = quiz.Questions.Select(MapToQuizQuestionDto).ToList(),
            CreatedAt = quiz.CreatedAt
        };
    }

    public QuizSessionDto MapToSessionDto(QuizSession session)
    {
        return new QuizSessionDto
        {
            Id = session.Id.Value,
            SessionName = session.SessionName,
            Topic = session.Topic,
            Book = session.Book,
            QuestionType = session.QuestionType.Value,
            Difficulty = session.Difficulty.Value,
            Language = session.Language.Name,
            Questions = session.Questions.Select(MapToQuizQuestionDto).ToList(),
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

    public QuizSession MapToSession(QuizSessionDto dto)
    {
        var session = new QuizSession
        {
            Id = !string.IsNullOrEmpty(dto.Id) ? SessionId.FromString(dto.Id) : SessionId.New(),
            SessionName = dto.SessionName,
            Topic = dto.Topic,
            Book = dto.Book,
            QuestionType = QuestionType.FromString(dto.QuestionType),
            Difficulty = DifficultyLevel.FromString(dto.Difficulty),
            Language = Language.FromString(dto.Language),
            CreatedAt = dto.CreatedAt != default ? dto.CreatedAt : DateTime.UtcNow,
            LastModifiedAt = dto.LastModifiedAt != default ? dto.LastModifiedAt : DateTime.UtcNow
        };

        foreach (var questionDto in dto.Questions)
        {
            session.AddQuestion(MapToQuestion(questionDto));
        }

        return session;
    }

    public SessionSummaryDto MapToSessionSummaryDto(SessionSummary summary)
    {
        return new SessionSummaryDto
        {
            Id = summary.Id.Value,
            SessionName = summary.SessionName,
            Topic = summary.Topic,
            Book = summary.Book,
            QuestionType = summary.QuestionType.Value,
            Difficulty = summary.Difficulty.Value,
            Language = summary.Language.Name,
            CreatedAt = summary.CreatedAt,
            LastModifiedAt = summary.LastModifiedAt,
            TotalQuestions = summary.TotalQuestions,
            AnsweredQuestions = summary.AnsweredQuestions,
            CorrectAnswers = summary.CorrectAnswers,
            IsCompleted = summary.IsCompleted,
            AverageScore = summary.AverageScore,
            AverageInterestRating = summary.AverageInterestRating
        };
    }

    public QuizQuestionDto MapToQuizQuestionDto(QuizQuestion question)
    {
        return new QuizQuestionDto
        {
            Id = question.Id.Value,
            Question = question.Question,
            Options = question.Options,
            CorrectAnswerIndex = question.CorrectAnswerIndex,
            Explanation = question.Explanation,
            Keywords = question.Keywords,
            UserAnswerIndex = question.UserAnswerIndex,
            IsCorrect = question.IsCorrect,
            UserInterestRating = question.UserInterestRating?.Value,
            CreatedAt = question.CreatedAt,
            AnsweredAt = question.AnsweredAt
        };
    }

    private QuizQuestion MapToQuestion(QuizQuestionDto dto)
    {
        var question = new QuizQuestion
        {
            Id = !string.IsNullOrEmpty(dto.Id) ? QuestionId.FromString(dto.Id) : QuestionId.New(),
            Question = dto.Question,
            Options = dto.Options,
            CorrectAnswerIndex = dto.CorrectAnswerIndex,
            Explanation = dto.Explanation,
            Keywords = dto.Keywords,
            UserAnswerIndex = dto.UserAnswerIndex,
            IsCorrect = dto.IsCorrect,
            UserInterestRating = InterestRating.FromNullableInt(dto.UserInterestRating),
            CreatedAt = dto.CreatedAt != default ? dto.CreatedAt : DateTime.UtcNow,
            AnsweredAt = dto.AnsweredAt
        };

        return question;
    }
} 