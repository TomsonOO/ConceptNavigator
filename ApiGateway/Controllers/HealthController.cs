using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
  [HttpGet]
  public IActionResult Get(){
    return Ok(new {Status = "Healthy", Service = "ConceptNavigator API Gateway", Timestamp = DateTime.UtcNow });
  }
}