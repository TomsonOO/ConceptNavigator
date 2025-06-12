using QuizGenerationService.Application.DTOs;

namespace QuizGenerationService.Application.Interfaces;

public interface ISessionManagementService
{
    Task<QuizSessionDto?> GetSessionAsync(string sessionId);
    Task<string> SaveSessionAsync(SaveSessionRequestDto request);
    Task<List<SessionSummaryDto>> GetSessionSummariesAsync();
    Task DeleteSessionAsync(string sessionId);
    Task RecordAnswerAsync(RecordAnswerRequestDto request);
    Task RecordInterestAsync(RecordInterestRequestDto request);
} 