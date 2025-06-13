using QuizGenerationService.Application.DTOs;
using QuizGenerationService.Application.Interfaces;
using QuizGenerationService.Domain.Interfaces;
using QuizGenerationService.Domain.Models;
using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Application.Services;

public class SessionManagementService : ISessionManagementService
{
    private readonly ISessionStorage _sessionStorage;
    private readonly IQuizMappingService _mappingService;
    private readonly ILogger<SessionManagementService> _logger;

    public SessionManagementService(
        ISessionStorage sessionStorage,
        IQuizMappingService mappingService,
        ILogger<SessionManagementService> logger)
    {
        _sessionStorage = sessionStorage;
        _mappingService = mappingService;
        _logger = logger;
    }

    public async Task<QuizSessionDto?> GetSessionAsync(string sessionId)
    {
        try
        {
            var sessionIdValue = SessionId.FromString(sessionId);
            var session = await _sessionStorage.GetSessionAsync(sessionIdValue);
            
            if (session == null)
            {
                _logger.LogWarning("Session not found: {SessionId}", sessionId);
                return null;
            }

            var dto = _mappingService.MapToSessionDto(session);
            _logger.LogDebug("Retrieved session {SessionId}", sessionId);
            
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<string> SaveSessionAsync(SaveSessionRequestDto request)
    {
        try
        {
            var session = _mappingService.MapToSession(request.Session);
            
            if (!string.IsNullOrWhiteSpace(request.SessionName))
            {
                session.SessionName = request.SessionName;
            }

            await _sessionStorage.SaveSessionAsync(session);
            
            _logger.LogInformation("Saved session {SessionId} with name '{SessionName}'", 
                session.Id, session.SessionName);
                
            return session.Id.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save session");
            throw;
        }
    }

    public async Task<List<SessionSummaryDto>> GetSessionSummariesAsync()
    {
        try
        {
            var summaries = await _sessionStorage.GetSessionSummariesAsync();
            var dtos = summaries.Select(_mappingService.MapToSessionSummaryDto).ToList();
            
            _logger.LogDebug("Retrieved {Count} session summaries", dtos.Count);
            
            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get session summaries");
            throw;
        }
    }

    public async Task DeleteSessionAsync(string sessionId)
    {
        try
        {
            var sessionIdValue = SessionId.FromString(sessionId);
            await _sessionStorage.DeleteSessionAsync(sessionIdValue);
            
            _logger.LogInformation("Deleted session {SessionId}", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task RecordAnswerAsync(RecordAnswerRequestDto request)
    {
        try
        {
            var sessionId = SessionId.FromString(request.SessionId);
            var questionId = QuestionId.FromString(request.QuestionId);
            
            var session = await _sessionStorage.GetSessionAsync(sessionId);
            if (session == null)
            {
                throw new InvalidOperationException($"Session not found: {request.SessionId}");
            }

            session.RecordAnswer(questionId, request.AnswerIndex);
            await _sessionStorage.SaveSessionAsync(session);
            
            _logger.LogDebug("Recorded answer for question {QuestionId} in session {SessionId}", 
                request.QuestionId, request.SessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record answer for question {QuestionId} in session {SessionId}", 
                request.QuestionId, request.SessionId);
            throw;
        }
    }

    public async Task RecordInterestAsync(RecordInterestRequestDto request)
    {
        try
        {
            var sessionId = SessionId.FromString(request.SessionId);
            var questionId = QuestionId.FromString(request.QuestionId);
            
            var session = await _sessionStorage.GetSessionAsync(sessionId);
            if (session == null)
            {
                throw new InvalidOperationException($"Session not found: {request.SessionId}");
            }

            session.RecordInterest(questionId, request.InterestRating);
            await _sessionStorage.SaveSessionAsync(session);
            
            _logger.LogDebug("Recorded interest rating {Rating} for question {QuestionId} in session {SessionId}", 
                request.InterestRating, request.QuestionId, request.SessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record interest for question {QuestionId} in session {SessionId}", 
                request.QuestionId, request.SessionId);
            throw;
        }
    }
} 