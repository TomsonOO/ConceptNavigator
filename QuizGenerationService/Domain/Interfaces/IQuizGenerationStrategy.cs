using QuizGenerationService.Domain.Models;

namespace QuizGenerationService.Domain.Interfaces;

public interface IQuizGenerationStrategy
{
    Task<Quiz> GenerateAsync(QuizGenerationRequest request);
    bool CanHandle(QuizGenerationRequest request);
} 