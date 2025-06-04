using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using ApiGateway.Models;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
  private readonly HttpClient _httpClient;
  private readonly string _quizServiceUrl;

  public QuizController(HttpClient httpClient, IConfiguration configuration)
  {
    _httpClient = httpClient;
    _quizServiceUrl = configuration["QUIZ_SERVICE_URL"] ?? "http://conceptnavigator-quiz-service/api/quiz";
  }

  [HttpPost("generate")]
  public async Task<ActionResult<QuizResponse>> Generate([FromBody] GenerateQuizRequest request)
  {
    var json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await _httpClient.PostAsync($"{_quizServiceUrl}/generate", content);
    var responseContent = await response.Content.ReadAsStringAsync();

    if (response.IsSuccessStatusCode)
    {
      var quizResponse = JsonSerializer.Deserialize<QuizResponse>(responseContent, new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      });
      return Ok(quizResponse);
    }

    return StatusCode((int)response.StatusCode, responseContent);
  }
}

public static class HttpRequestExtensions
{
  public static async Task<string> GetRawBodyStringAsync(this HttpRequest request)
  {
    using var reader = new StreamReader(request.Body);
    return await reader.ReadToEndAsync();
  }
}