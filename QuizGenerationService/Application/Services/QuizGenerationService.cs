using QuizGenerationService.Application.DTOs;
using QuizGenerationService.Application.Interfaces;
using QuizGenerationService.Application.Validation;
using QuizGenerationService.Domain.Interfaces;
using QuizGenerationService.Domain.Models;
using QuizGenerationService.Infrastructure.Factories;

namespace QuizGenerationService.Application.Services;

public class QuizGenerationService : IQuizGenerationService
{
    private readonly QuizGenerationStrategyFactory _strategyFactory;
    private readonly IQuizMappingService _mappingService;
    private readonly QuizRequestValidator _validator;
    private readonly ISessionStorage _sessionStorage;
    private readonly ILogger<QuizGenerationService> _logger;

    public QuizGenerationService(
        QuizGenerationStrategyFactory strategyFactory,
        IQuizMappingService mappingService,
        QuizRequestValidator validator,
        ISessionStorage sessionStorage,
        ILogger<QuizGenerationService> logger)
    {
        _strategyFactory = strategyFactory;
        _mappingService = mappingService;
        _validator = validator;
        _sessionStorage = sessionStorage;
        _logger = logger;
    }

    public async Task<QuizResponseDto> GenerateQuizAsync(QuizRequestDto request)
    {
        _logger.LogInformation("Starting quiz generation for topic: {Topic}", request.Topic);

        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join("; ", validationResult.Errors);
            _logger.LogWarning("Validation failed for quiz request: {Errors}", errorMessage);
            throw new ArgumentException($"Invalid request: {errorMessage}");
        }

        try
        {
            var domainRequest = _mappingService.MapToRequest(request);
            
            var strategy = _strategyFactory.GetStrategy(domainRequest);
            
            var quiz = await strategy.GenerateAsync(domainRequest);
            
            var session = CreateSessionFromQuiz(quiz, domainRequest);
            
            await _sessionStorage.SaveSessionAsync(session);
            
            var response = _mappingService.MapToResponse(quiz);
            response.SessionId = session.Id.Value;
            
            _logger.LogInformation("Successfully generated quiz for topic: {Topic} with {QuestionCount} questions and created session {SessionId}", 
                quiz.Topic, quiz.Questions.Count, session.Id.Value);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate quiz for topic: {Topic}", request.Topic);
            throw;
        }
    }

    private QuizSession CreateSessionFromQuiz(Quiz quiz, QuizGenerationRequest request)
    {
        var session = new QuizSession
        {
            Topic = quiz.Topic,
            Book = quiz.Book,
            QuestionType = request.QuestionType,
            Difficulty = request.Difficulty,
            Language = request.Language
        };

        session.AddQuestions(quiz.Questions);
        
        return session;
    }
} 