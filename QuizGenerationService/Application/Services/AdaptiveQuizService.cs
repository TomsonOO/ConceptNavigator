using QuizGenerationService.Application.DTOs;
using QuizGenerationService.Application.Interfaces;
using QuizGenerationService.Application.Validation;
using QuizGenerationService.Domain.Interfaces;
using QuizGenerationService.Domain.Models;
using QuizGenerationService.Domain.ValueObjects;
using QuizGenerationService.Infrastructure.ExternalServices;
using QuizGenerationService.Infrastructure.PromptBuilding;

namespace QuizGenerationService.Application.Services;

public class AdaptiveQuizService : IAdaptiveQuizService
{
    private readonly ISessionStorage _sessionStorage;
    private readonly KeywordExtractionApiService _keywordExtractionService;
    private readonly GeminiApiService _geminiApiService;
    private readonly AdaptivePromptBuilder _promptBuilder;
    private readonly IQuizMappingService _mappingService;
    private readonly GenerateMoreQuestionsValidator _validator;
    private readonly ILogger<AdaptiveQuizService> _logger;

    public AdaptiveQuizService(
        ISessionStorage sessionStorage,
        KeywordExtractionApiService keywordExtractionService,
        GeminiApiService geminiApiService,
        AdaptivePromptBuilder promptBuilder,
        IQuizMappingService mappingService,
        GenerateMoreQuestionsValidator validator,
        ILogger<AdaptiveQuizService> logger)
    {
        _sessionStorage = sessionStorage;
        _keywordExtractionService = keywordExtractionService;
        _geminiApiService = geminiApiService;
        _promptBuilder = promptBuilder;
        _mappingService = mappingService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<AdaptiveQuizResponseDto> GenerateAdaptiveQuestionsAsync(GenerateMoreQuestionsRequestDto request)
    {
        try
        {
            _logger.LogInformation("Starting adaptive quiz generation for session: {SessionId}", request.SessionId);

            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join("; ", validationResult.Errors);
                _logger.LogWarning("Validation failed for adaptive quiz request: {Errors}", errorMessage);
                throw new ArgumentException($"Invalid request: {errorMessage}");
            }

            var session = await LoadSessionAsync(request.SessionId);
            var adaptiveRequest = await BuildAdaptiveRequestAsync(request, session);
            
            var quiz = await GeneratePersonalizedQuizAsync(adaptiveRequest);
            
            await UpdateSessionWithNewQuestionsAsync(session, quiz.Questions);
            
            var response = MapToResponseDto(quiz, adaptiveRequest);
            
            _logger.LogInformation("Successfully generated {QuestionCount} adaptive questions for session {SessionId}, " +
                                 "personalization: {WasPersonalized}, adaptation score: {Score:F2}", 
                                 response.Questions.Count, request.SessionId, 
                                 response.WasPersonalized, response.AdaptationScore);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate adaptive questions for session: {SessionId}", request.SessionId);
            throw;
        }
    }

    private async Task<QuizSession> LoadSessionAsync(string sessionId)
    {
        var sessionIdValue = SessionId.FromString(sessionId);
        var session = await _sessionStorage.GetSessionAsync(sessionIdValue);
        
        if (session == null)
        {
            throw new ArgumentException($"Session not found: {sessionId}");
        }

        return session;
    }

    private async Task<AdaptiveQuizRequest> BuildAdaptiveRequestAsync(GenerateMoreQuestionsRequestDto request, QuizSession session)
    {
        var adaptiveRequest = new AdaptiveQuizRequest
        {
            SessionId = session.Id,
            Topic = session.Topic,
            Book = session.Book,
            QuestionType = !string.IsNullOrWhiteSpace(request.QuestionType) 
                ? QuestionType.FromString(request.QuestionType) 
                : session.QuestionType,
            Difficulty = !string.IsNullOrWhiteSpace(request.Difficulty) 
                ? DifficultyLevel.FromString(request.Difficulty) 
                : session.Difficulty,
            Language = !string.IsNullOrWhiteSpace(request.Language) 
                ? Language.FromString(request.Language) 
                : session.Language,
            QuestionCount = request.QuestionCount,
            MinInterestThreshold = request.MinInterestThreshold,
            FocusArea = request.FocusArea,
            IncludeRelatedConcepts = request.IncludeRelatedConcepts,
            AvoidRepeatedTopics = request.AvoidRepeatedTopics
        };

        foreach (var interest in request.AdditionalInterests)
        {
            adaptiveRequest.AddUserInterest(interest);
        }

        var existingInterests = session.GetHighInterestKeywords();
        foreach (var interest in existingInterests)
        {
            adaptiveRequest.AddUserInterest(interest);
        }

        await ExtractAndAddKeywordsAsync(adaptiveRequest, session);

        return adaptiveRequest;
    }

    private async Task ExtractAndAddKeywordsAsync(AdaptiveQuizRequest adaptiveRequest, QuizSession session)
    {
        try
        {
            var keywordExtraction = await _keywordExtractionService.ExtractKeywordsFromSessionAsync(session);
            
            var highPriorityKeywords = keywordExtraction.GetHighPriorityKeywords(10);
            foreach (var keyword in highPriorityKeywords)
            {
                adaptiveRequest.AddPriorityKeyword(keyword);
            }

            _logger.LogDebug("Added {KeywordCount} priority keywords to adaptive request for session {SessionId}", 
                           highPriorityKeywords.Count, session.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract keywords for session {SessionId}, continuing without keyword enhancement", session.Id);
        }
    }

    private async Task<Quiz> GeneratePersonalizedQuizAsync(AdaptiveQuizRequest adaptiveRequest)
    {
        var prompt = _promptBuilder.BuildAdaptivePrompt(adaptiveRequest);
        
        _logger.LogDebug("Generated adaptive prompt for session {SessionId}, keywords: {KeywordCount}, interests: {InterestCount}", 
                       adaptiveRequest.SessionId, adaptiveRequest.PriorityKeywords.Count, adaptiveRequest.UserInterests.Count);

        var quiz = await _geminiApiService.GenerateQuizAsync(prompt);
        
        foreach (var question in quiz.Questions)
        {
            question.Id = QuestionId.New();
            question.CreatedAt = DateTime.UtcNow;
        }

        return quiz;
    }

    private async Task UpdateSessionWithNewQuestionsAsync(QuizSession session, List<QuizQuestion> newQuestions)
    {
        foreach (var question in newQuestions)
        {
            session.AddQuestion(question);
        }

        await _sessionStorage.SaveSessionAsync(session);
        
        _logger.LogDebug("Added {QuestionCount} new questions to session {SessionId}, total: {TotalQuestions}", 
                       newQuestions.Count, session.Id, session.Questions.Count);
    }

    private AdaptiveQuizResponseDto MapToResponseDto(Quiz quiz, AdaptiveQuizRequest adaptiveRequest)
    {
        var questionDtos = quiz.Questions.Select(_mappingService.MapToQuizQuestionDto).ToList();
        
        var keywordsUsed = adaptiveRequest.GetTopKeywords(5);
        var adaptationScore = CalculateAdaptationScore(adaptiveRequest);
        var wasPersonalized = adaptiveRequest.ShouldFocusOnKeywords || adaptiveRequest.HasUserPreferences;
        var personalizationReason = BuildPersonalizationReason(adaptiveRequest);

        return new AdaptiveQuizResponseDto
        {
            SessionId = adaptiveRequest.SessionId.Value,
            Topic = adaptiveRequest.Topic,
            Book = adaptiveRequest.Book,
            QuestionType = adaptiveRequest.QuestionType.Value,
            Difficulty = adaptiveRequest.Difficulty.Value,
            Language = adaptiveRequest.Language.Name,
            Questions = questionDtos,
            CreatedAt = adaptiveRequest.CreatedAt,
            KeywordsUsed = keywordsUsed,
            UserInterestsApplied = adaptiveRequest.UserInterests,
            FocusArea = adaptiveRequest.FocusArea,
            AdaptationScore = adaptationScore,
            WasPersonalized = wasPersonalized,
            PersonalizationReason = personalizationReason
        };
    }

    private double CalculateAdaptationScore(AdaptiveQuizRequest request)
    {
        double score = 0.0;
        
        if (request.PriorityKeywords.Any())
            score += 0.4;
        
        if (request.UserInterests.Any())
            score += 0.3;
        
        if (!string.IsNullOrWhiteSpace(request.FocusArea))
            score += 0.2;
        
        if (request.IncludeRelatedConcepts)
            score += 0.1;

        return Math.Min(1.0, score);
    }

    private string BuildPersonalizationReason(AdaptiveQuizRequest request)
    {
        var reasons = new List<string>();
        
        if (request.PriorityKeywords.Any())
            reasons.Add($"extracted {request.PriorityKeywords.Count} high-interest keywords");
        
        if (request.UserInterests.Any())
            reasons.Add($"applied {request.UserInterests.Count} user interests");
        
        if (!string.IsNullOrWhiteSpace(request.FocusArea))
            reasons.Add("focused on specific area");
        
        if (request.IncludeRelatedConcepts)
            reasons.Add("included related concepts");

        return reasons.Any() ? string.Join(", ", reasons) : "no personalization applied";
    }
} 