using QuizGenerationService.Application.DTOs;
using QuizGenerationService.Application.Interfaces;
using QuizGenerationService.Application.Mapping;
using QuizGenerationService.Application.Validation;
using QuizGenerationService.Infrastructure.Factories;

namespace QuizGenerationService.Application.Services;

public class QuizGenerationService : IQuizGenerationService
{
    private readonly QuizGenerationStrategyFactory _strategyFactory;
    private readonly QuizMappingService _mappingService;
    private readonly QuizRequestValidator _validator;
    private readonly ILogger<QuizGenerationService> _logger;

    public QuizGenerationService(
        QuizGenerationStrategyFactory strategyFactory,
        QuizMappingService mappingService,
        QuizRequestValidator validator,
        ILogger<QuizGenerationService> logger)
    {
        _strategyFactory = strategyFactory;
        _mappingService = mappingService;
        _validator = validator;
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
            
            var response = _mappingService.MapToResponse(quiz);
            
            _logger.LogInformation("Successfully generated quiz for topic: {Topic} with {QuestionCount} questions", 
                quiz.Topic, quiz.Questions.Count);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate quiz for topic: {Topic}", request.Topic);
            throw;
        }
    }
} 