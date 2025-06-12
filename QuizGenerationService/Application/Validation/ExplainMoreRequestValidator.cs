using QuizGenerationService.Application.DTOs;
using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Application.Validation;

public class ExplainMoreRequestValidator
{
    public ValidationResult Validate(ExplainMoreRequestDto request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.QuestionId))
            errors.Add("QuestionId is required");

        if (string.IsNullOrWhiteSpace(request.QuestionText) && string.IsNullOrWhiteSpace(request.SessionId))
            errors.Add("Either QuestionText or SessionId must be provided");

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

        if (request.UserInterests != null && request.UserInterests.Count > 20)
            errors.Add("Maximum 20 user interests allowed");

        if (!string.IsNullOrWhiteSpace(request.FocusArea) && request.FocusArea.Length > 500)
            errors.Add("Focus area must be 500 characters or less");

        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }
} 