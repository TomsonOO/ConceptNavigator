using QuizGenerationService.Application.DTOs;

namespace QuizGenerationService.Application.Interfaces;

public interface IConceptMapService
{
    Task<ConceptMapDto> GenerateConceptMapAsync(GenerateConceptMapRequestDto request);
} 