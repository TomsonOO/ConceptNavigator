using Microsoft.AspNetCore.Mvc;
using QuizGenerationService.Application.DTOs;
using QuizGenerationService.Application.Interfaces;

namespace QuizGenerationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly IQuizGenerationService _quizService;
    private readonly IExtendedExplanationService _explanationService;
    private readonly IAdaptiveQuizService _adaptiveQuizService;
    private readonly ILogger<QuizController> _logger;

    public QuizController(
        IQuizGenerationService quizService, 
        IExtendedExplanationService explanationService,
        IAdaptiveQuizService adaptiveQuizService,
        ILogger<QuizController> logger)
    {
        _quizService = quizService;
        _explanationService = explanationService;
        _adaptiveQuizService = adaptiveQuizService;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<QuizResponseDto>> GenerateQuiz([FromBody] QuizRequestDto request)
    {
        try
        {
            var response = await _quizService.GenerateQuizAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for quiz generation");
            return BadRequest($"Invalid request: {ex.Message}");
        }
        catch (NotSupportedException ex)
        {
            _logger.LogWarning(ex, "Unsupported configuration for quiz generation");
            return BadRequest($"Unsupported configuration: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during quiz generation");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("explain-more")]
    public async Task<ActionResult<ExtendedExplanationDto>> ExplainMore([FromBody] ExplainMoreRequestDto request)
    {
        try
        {
            var explanation = await _explanationService.GenerateExtendedExplanationAsync(request);
            return Ok(explanation);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for extended explanation: {QuestionId}", request.QuestionId);
            return BadRequest($"Invalid request: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to generate extended explanation: {QuestionId}", request.QuestionId);
            return BadRequest($"Generation failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during extended explanation generation: {QuestionId}", request.QuestionId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("generate-more-questions")]
    public async Task<ActionResult<AdaptiveQuizResponseDto>> GenerateMoreQuestions([FromBody] GenerateMoreQuestionsRequestDto request)
    {
        try
        {
            var response = await _adaptiveQuizService.GenerateAdaptiveQuestionsAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for adaptive quiz generation: {SessionId}", request.SessionId);
            return BadRequest($"Invalid request: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to generate adaptive quiz: {SessionId}", request.SessionId);
            return BadRequest($"Generation failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during adaptive quiz generation: {SessionId}", request.SessionId);
            return StatusCode(500, "Internal server error");
        }
    }
}

