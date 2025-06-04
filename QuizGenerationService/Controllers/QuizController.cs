using Microsoft.AspNetCore.Mvc;
using QuizGenerationService.Handlers;
using QuizGenerationService.Queries;

namespace QuizGenerationService.Controllers;

[ApiController]
[Route("api/quiz")]
public class QuizController : ControllerBase
{
  private readonly GenerateQuizHandler _generateQuizHandler;

  public QuizController(GenerateQuizHandler generateQuizHandler)
  {
    _generateQuizHandler = generateQuizHandler;
  }

  [HttpPost("generate")]
  public async Task<IActionResult> GenerateQuiz(GenerateQuizQuery query)
  {
    var quiz = await _generateQuizHandler.Handle(query);
    return Ok(quiz);
  }
}

