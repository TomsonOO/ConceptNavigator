using QuizGenerationService.Domain.Interfaces;
using QuizGenerationService.Domain.Models;
using QuizGenerationService.Domain.ValueObjects;
using QuizGenerationService.Infrastructure.ExternalServices;

namespace QuizGenerationService.Infrastructure.Strategies;

public class BasicQuizGenerationStrategy : IQuizGenerationStrategy
{
    private readonly GeminiApiService _geminiApiService;
    private readonly IPromptBuilder _promptBuilder;
    private readonly ILogger<BasicQuizGenerationStrategy> _logger;

    public BasicQuizGenerationStrategy(
        GeminiApiService geminiApiService,
        IPromptBuilder promptBuilder,
        ILogger<BasicQuizGenerationStrategy> logger)
    {
        _geminiApiService = geminiApiService;
        _promptBuilder = promptBuilder;
        _logger = logger;
    }

    public bool CanHandle(QuizGenerationRequest request)
    {
        return request.QuestionType.Equals(QuestionType.Basic);
    }

    public async Task<Quiz> GenerateAsync(QuizGenerationRequest request)
    {
        _logger.LogInformation("Generating basic quiz for topic: {Topic}", request.Topic);

        var prompt = _promptBuilder.BuildPrompt(request);
        var quiz = await _geminiApiService.GenerateQuizAsync(prompt);
        
        PopulateQuizMetadata(quiz, request);
        
        _logger.LogInformation("Successfully generated basic quiz with {QuestionCount} questions", quiz.Questions.Count);
        
        return quiz;
    }

    private void PopulateQuizMetadata(Quiz quiz, QuizGenerationRequest request)
    {
        quiz.Book = request.Book;
        quiz.QuestionType = request.QuestionType;
        quiz.Difficulty = request.Difficulty;
        quiz.Language = request.Language;
        quiz.CreatedAt = DateTime.UtcNow;
    }
} 