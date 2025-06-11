using QuizGenerationService.Application.DTOs;
using QuizGenerationService.Domain.Models;
using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Application.Mapping;

public class QuizMappingService
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
            Questions = quiz.Questions.Select(MapToQuestionDto).ToList(),
            CreatedAt = quiz.CreatedAt
        };
    }

    private QuizQuestionDto MapToQuestionDto(QuizQuestion question)
    {
        return new QuizQuestionDto
        {
            Question = question.Question,
            Options = question.Options,
            CorrectAnswerIndex = question.CorrectAnswerIndex,
            Explanation = question.Explanation
        };
    }
} 