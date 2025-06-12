using QuizGenerationService.Application.DTOs;
using QuizGenerationService.Application.Interfaces;
using QuizGenerationService.Application.Validation;
using QuizGenerationService.Domain.Interfaces;
using QuizGenerationService.Domain.ValueObjects;
using QuizGenerationService.Infrastructure.ExternalServices;

namespace QuizGenerationService.Application.Services;

public class ExtendedExplanationService : IExtendedExplanationService
{
    private readonly ExtendedExplanationApiService _apiService;
    private readonly ISessionStorage _sessionStorage;
    private readonly IQuizMappingService _mappingService;
    private readonly ExplainMoreRequestValidator _validator;
    private readonly ILogger<ExtendedExplanationService> _logger;

    public ExtendedExplanationService(
        ExtendedExplanationApiService apiService,
        ISessionStorage sessionStorage,
        IQuizMappingService mappingService,
        ExplainMoreRequestValidator validator,
        ILogger<ExtendedExplanationService> logger)
    {
        _apiService = apiService;
        _sessionStorage = sessionStorage;
        _mappingService = mappingService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ExtendedExplanationDto> GenerateExtendedExplanationAsync(ExplainMoreRequestDto request)
    {
        try
        {
            _logger.LogInformation("Starting extended explanation generation for question: {QuestionId}", request.QuestionId);

            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join("; ", validationResult.Errors);
                _logger.LogWarning("Validation failed for extended explanation request: {Errors}", errorMessage);
                throw new ArgumentException($"Invalid request: {errorMessage}");
            }

            var context = await GatherContextAsync(request);
            
            var difficulty = DifficultyLevel.FromString(request.Difficulty);
            var language = Language.FromString(request.Language);

            var explanation = await _apiService.GenerateExtendedExplanationAsync(
                context.QuestionText,
                context.OriginalExplanation,
                context.Topic,
                context.Book,
                difficulty,
                language,
                context.UserInterests,
                request.FocusArea);

            explanation.QuestionId = QuestionId.FromString(request.QuestionId);

            var dto = MapToDto(explanation);
            
            _logger.LogInformation("Successfully generated extended explanation for question: {QuestionId}, " +
                                 "paragraphs: {Paragraphs}, reading time: {Minutes} min", 
                                 request.QuestionId, dto.TotalParagraphs, dto.EstimatedReadingTimeMinutes);

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate extended explanation for question: {QuestionId}", request.QuestionId);
            throw;
        }
    }

    private async Task<ExplanationContext> GatherContextAsync(ExplainMoreRequestDto request)
    {
        var context = new ExplanationContext
        {
            QuestionText = request.QuestionText ?? string.Empty,
            OriginalExplanation = request.OriginalExplanation ?? string.Empty,
            Topic = request.Topic ?? "Philosophy",
            Book = request.Book,
            UserInterests = request.UserInterests ?? new List<string>()
        };

        if (!string.IsNullOrWhiteSpace(request.SessionId))
        {
            try
            {
                var sessionId = SessionId.FromString(request.SessionId);
                var session = await _sessionStorage.GetSessionAsync(sessionId);
                
                if (session != null)
                {
                    context.Topic = session.Topic;
                    context.Book = session.Book;
                    
                    if (string.IsNullOrWhiteSpace(context.QuestionText))
                    {
                        var questionId = QuestionId.FromString(request.QuestionId);
                        var question = session.GetQuestion(questionId);
                        if (question != null)
                        {
                            context.QuestionText = question.Question;
                            context.OriginalExplanation = question.Explanation;
                        }
                    }

                    var highInterestKeywords = session.GetHighInterestKeywords();
                    context.UserInterests = context.UserInterests.Concat(highInterestKeywords).Distinct().ToList();
                    
                    _logger.LogDebug("Enhanced context from session {SessionId}: topic={Topic}, interests={InterestCount}", 
                                   request.SessionId, context.Topic, context.UserInterests.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load session context for {SessionId}, using provided context", request.SessionId);
            }
        }

        if (string.IsNullOrWhiteSpace(context.QuestionText))
        {
            throw new ArgumentException("Question text is required either in request or session");
        }

        return context;
    }

    private ExtendedExplanationDto MapToDto(Domain.Models.ExtendedExplanation explanation)
    {
        return new ExtendedExplanationDto
        {
            QuestionId = explanation.QuestionId.Value,
            QuestionText = explanation.QuestionText,
            MainExplanation = explanation.MainExplanation,
            DetailedParagraphs = explanation.DetailedParagraphs,
            HistoricalContext = explanation.HistoricalContext,
            ContemporaryRelevance = explanation.ContemporaryRelevance,
            KeyConcepts = explanation.KeyConcepts,
            RelatedPhilosophers = explanation.RelatedPhilosophers,
            SuggestedSources = explanation.SuggestedSources.Select(MapToSourceDto).ToList(),
            CreatedAt = explanation.CreatedAt,
            OriginalDifficulty = explanation.OriginalDifficulty.Value,
            Language = explanation.Language.Name,
            TotalParagraphs = explanation.TotalParagraphs,
            EstimatedReadingTimeMinutes = explanation.EstimatedReadingTimeMinutes
        };
    }

    private SuggestedSourceDto MapToSourceDto(Domain.Models.SuggestedSource source)
    {
        return new SuggestedSourceDto
        {
            Title = source.Title,
            Author = source.Author,
            Type = source.Type,
            Description = source.Description,
            RelevanceScore = source.RelevanceScore
        };
    }

    private class ExplanationContext
    {
        public string QuestionText { get; set; } = string.Empty;
        public string OriginalExplanation { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string? Book { get; set; }
        public List<string> UserInterests { get; set; } = new();
    }
} 