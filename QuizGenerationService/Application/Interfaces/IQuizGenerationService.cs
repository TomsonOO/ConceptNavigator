using QuizGenerationService.Application.DTOs;

namespace QuizGenerationService.Application.Interfaces;

public interface IQuizGenerationService
{
    Task<QuizResponseDto> GenerateQuizAsync(QuizRequestDto request);
} 