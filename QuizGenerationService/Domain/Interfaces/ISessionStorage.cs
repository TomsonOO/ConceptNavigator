using QuizGenerationService.Domain.Models;
using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Domain.Interfaces;

public interface ISessionStorage
{
    Task<QuizSession?> GetSessionAsync(SessionId sessionId);
    Task SaveSessionAsync(QuizSession session);
    Task<List<SessionSummary>> GetSessionSummariesAsync();
    Task DeleteSessionAsync(SessionId sessionId);
    Task<bool> ExistsAsync(SessionId sessionId);
} 