using QuizGenerationService.Application.DTOs;
using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Application.Validation;

public class GenerateMoreQuestionsValidator
{
    public ValidationResult Validate(GenerateMoreQuestionsRequestDto request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.SessionId))
            errors.Add("SessionId is required");

        if (request.QuestionCount < 1 || request.QuestionCount > 10)
            errors.Add("QuestionCount must be between 1 and 10");

        if (!string.IsNullOrWhiteSpace(request.QuestionType))
        {
            try
            {
                QuestionType.FromString(request.QuestionType);
            }
            catch
            {
                errors.Add($"Invalid question type: {request.QuestionType}. Valid values: Basic, Situational");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Difficulty))
        {
            try
            {
                DifficultyLevel.FromString(request.Difficulty);
            }
            catch
            {
                errors.Add($"Invalid difficulty level: {request.Difficulty}. Valid values: Easy, Medium, Hard");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Language))
        {
            try
            {
                Language.FromString(request.Language);
            }
            catch
            {
                errors.Add($"Invalid language: {request.Language}. Valid values: Polish, English");
            }
        }

        if (request.MinInterestThreshold < 0.0 || request.MinInterestThreshold > 1.0)
            errors.Add("MinInterestThreshold must be between 0.0 and 1.0");

        if (request.AdditionalInterests != null && request.AdditionalInterests.Count > 15)
            errors.Add("Maximum 15 additional interests allowed");

        if (!string.IsNullOrWhiteSpace(request.FocusArea) && request.FocusArea.Length > 300)
            errors.Add("Focus area must be 300 characters or less");

        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }
} 