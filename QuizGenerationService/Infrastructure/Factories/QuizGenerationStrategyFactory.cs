using QuizGenerationService.Domain.Interfaces;
using QuizGenerationService.Domain.Models;

namespace QuizGenerationService.Infrastructure.Factories;

public class QuizGenerationStrategyFactory
{
    private readonly IEnumerable<IQuizGenerationStrategy> _strategies;

    public QuizGenerationStrategyFactory(IEnumerable<IQuizGenerationStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IQuizGenerationStrategy GetStrategy(QuizGenerationRequest request)
    {
        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(request));
        
        if (strategy == null)
        {
            throw new NotSupportedException($"No strategy found for question type: {request.QuestionType}");
        }

        return strategy;
    }
} 