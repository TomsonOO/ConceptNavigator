using Microsoft.AspNetCore.Mvc;
using QuizGenerationService.Application.DTOs;
using QuizGenerationService.Application.Interfaces;

namespace QuizGenerationService.Controllers;

[ApiController]
[Route("api/quiz")]
public class QuizController : ControllerBase
{
    private readonly IQuizGenerationService _quizGenerationService;
    private readonly ILogger<QuizController> _logger;

    public QuizController(IQuizGenerationService quizGenerationService, ILogger<QuizController> logger)
    {
        _quizGenerationService = quizGenerationService;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateQuiz([FromBody] QuizRequestDto request)
    {
        try
        {
            _logger.LogInformation("Received quiz generation request for topic: {Topic}", request.Topic);
            
            var response = await _quizGenerationService.GenerateQuizAsync(request);
            
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid request: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (NotSupportedException ex)
        {
            _logger.LogWarning("Unsupported request: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during quiz generation");
            return StatusCode(500, new { error = "An unexpected error occurred while generating the quiz" });
        }
    }
}

