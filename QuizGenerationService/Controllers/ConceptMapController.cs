using Microsoft.AspNetCore.Mvc;
using QuizGenerationService.Application.DTOs;
using QuizGenerationService.Application.Interfaces;

namespace QuizGenerationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConceptMapController : ControllerBase
{
    private readonly IConceptMapService _conceptMapService;
    private readonly ILogger<ConceptMapController> _logger;

    public ConceptMapController(
        IConceptMapService conceptMapService,
        ILogger<ConceptMapController> logger)
    {
        _conceptMapService = conceptMapService;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<ConceptMapDto>> GenerateConceptMap([FromBody] GenerateConceptMapRequestDto request)
    {
        try
        {
            var conceptMap = await _conceptMapService.GenerateConceptMapAsync(request);
            return Ok(conceptMap);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for concept map generation");
            return BadRequest($"Invalid request: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to generate concept map");
            return BadRequest($"Generation failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during concept map generation");
            return StatusCode(500, "Internal server error");
        }
    }
} 