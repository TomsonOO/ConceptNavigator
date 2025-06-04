using Microsoft.AspNetCore.Mvc;

namespace QuizGenerationService.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
  [HttpGet]
  public IActionResult Get(){
    return Ok(new {Status = "Healthy", Service = "ConceptNavigator Quiz Generation Service", Timestamp = DateTime.UtcNow });
  }
}

