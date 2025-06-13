using QuizGenerationService.Application.DTOs;
using QuizGenerationService.Domain.Models;

namespace QuizGenerationService.Application.Interfaces;

public interface IQuizMappingService
{
    QuizGenerationRequest MapToRequest(QuizRequestDto dto);
    QuizResponseDto MapToResponse(Quiz quiz);
    QuizSessionDto MapToSessionDto(QuizSession session);
    QuizSession MapToSession(QuizSessionDto dto);
    SessionSummaryDto MapToSessionSummaryDto(SessionSummary summary);
    QuizQuestionDto MapToQuizQuestionDto(QuizQuestion question);
} 