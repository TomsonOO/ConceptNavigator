using QuizGenerationService.Application.DTOs;
using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Application.Validation;

public class GenerateConceptMapValidator
{
    public ValidationResult Validate(GenerateConceptMapRequestDto request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.SessionId) && string.IsNullOrWhiteSpace(request.Topic))
            errors.Add("Either SessionId or Topic must be provided");

        if (request.MaxNodes < 5 || request.MaxNodes > 30)
            errors.Add("MaxNodes must be between 5 and 30");

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

        if (request.FocusKeywords != null && request.FocusKeywords.Count > 20)
            errors.Add("Maximum 20 focus keywords allowed");

        if (request.MinImportanceThreshold < 0.0 || request.MinImportanceThreshold > 1.0)
            errors.Add("MinImportanceThreshold must be between 0.0 and 1.0");

        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }
} 