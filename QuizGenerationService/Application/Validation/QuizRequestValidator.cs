using QuizGenerationService.Application.DTOs;

namespace QuizGenerationService.Application.Validation;

public class QuizRequestValidator
{
    public ValidationResult Validate(QuizRequestDto request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Topic))
        {
            errors.Add("Topic is required and cannot be empty");
        }

        if (request.Topic.Length > 200)
        {
            errors.Add("Topic cannot exceed 200 characters");
        }

        if (request.QuestionCount < 1 || request.QuestionCount > 20)
        {
            errors.Add("Question count must be between 1 and 20");
        }

        if (!string.IsNullOrEmpty(request.Book) && request.Book.Length > 200)
        {
            errors.Add("Book name cannot exceed 200 characters");
        }

        var validQuestionTypes = new[] { "basic", "situational" };
        if (!validQuestionTypes.Contains(request.QuestionType.ToLower()))
        {
            errors.Add($"Question type must be one of: {string.Join(", ", validQuestionTypes)}");
        }

        var validDifficulties = new[] { "easy", "medium", "hard" };
        if (!validDifficulties.Contains(request.Difficulty.ToLower()))
        {
            errors.Add($"Difficulty must be one of: {string.Join(", ", validDifficulties)}");
        }

        var validLanguages = new[] { "polish", "pl", "english", "en" };
        if (!validLanguages.Contains(request.Language.ToLower()))
        {
            errors.Add($"Language must be one of: {string.Join(", ", validLanguages)}");
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
} 