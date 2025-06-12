using QuizGenerationService.Application.DTOs;

namespace QuizGenerationService.Application.Interfaces;

public interface IExtendedExplanationService
{
    Task<ExtendedExplanationDto> GenerateExtendedExplanationAsync(ExplainMoreRequestDto request);
} 