using QuizGenerationService.Application.DTOs;

namespace QuizGenerationService.Application.Interfaces;

public interface IAdaptiveQuizService
{
    Task<AdaptiveQuizResponseDto> GenerateAdaptiveQuestionsAsync(GenerateMoreQuestionsRequestDto request);
} 