using Microsoft.AspNetCore.Mvc;
using QuizGenerationService.Application.DTOs;
using QuizGenerationService.Application.Interfaces;

namespace QuizGenerationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly ISessionManagementService _sessionService;
    private readonly ILogger<SessionController> _logger;

    public SessionController(ISessionManagementService sessionService, ILogger<SessionController> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    [HttpGet("{sessionId}")]
    public async Task<ActionResult<QuizSessionDto>> GetSession(string sessionId)
    {
        try
        {
            var session = await _sessionService.GetSessionAsync(sessionId);
            
            if (session == null)
            {
                return NotFound($"Session {sessionId} not found");
            }

            return Ok(session);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid session ID format: {SessionId}", sessionId);
            return BadRequest($"Invalid session ID format: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session {SessionId}", sessionId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<string>> SaveSession([FromBody] SaveSessionRequestDto request)
    {
        try
        {
            var sessionId = await _sessionService.SaveSessionAsync(request);
            return Ok(sessionId);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid session data");
            return BadRequest($"Invalid session data: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving session");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<SessionSummaryDto>>> GetSessionSummaries()
    {
        try
        {
            var summaries = await _sessionService.GetSessionSummariesAsync();
            return Ok(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session summaries");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{sessionId}")]
    public async Task<ActionResult> DeleteSession(string sessionId)
    {
        try
        {
            await _sessionService.DeleteSessionAsync(sessionId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid session ID format: {SessionId}", sessionId);
            return BadRequest($"Invalid session ID format: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session {SessionId}", sessionId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("record-answer")]
    public async Task<ActionResult> RecordAnswer([FromBody] RecordAnswerRequestDto request)
    {
        try
        {
            await _sessionService.RecordAnswerAsync(request);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Session or question not found");
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data");
            return BadRequest($"Invalid request data: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording answer");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("record-interest")]
    public async Task<ActionResult> RecordInterest([FromBody] RecordInterestRequestDto request)
    {
        try
        {
            await _sessionService.RecordInterestAsync(request);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Session or question not found");
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data");
            return BadRequest($"Invalid request data: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording interest");
            return StatusCode(500, "Internal server error");
        }
    }
} 
