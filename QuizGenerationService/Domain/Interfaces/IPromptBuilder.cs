using QuizGenerationService.Domain.Models;

namespace QuizGenerationService.Domain.Interfaces;

public interface IPromptBuilder
{
    string BuildPrompt(QuizGenerationRequest request);
} 