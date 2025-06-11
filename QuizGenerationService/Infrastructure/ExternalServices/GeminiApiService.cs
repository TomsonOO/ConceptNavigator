using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using QuizGenerationService.Domain.Models;

namespace QuizGenerationService.Infrastructure.ExternalServices;

public class GeminiApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _model;
    private readonly ILogger<GeminiApiService> _logger;

    public GeminiApiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiApiService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GEMINI_API_KEY"] ?? configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not configured");
        _baseUrl = configuration["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta";
        _model = configuration["GEMINI_MODEL"] ?? configuration["Gemini:Model"] ?? "gemini-2.0-flash";
        _logger = logger;
        
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<Quiz> GenerateQuizAsync(string prompt)
    {
        try
        {
            var requestBody = CreateRequestBody(prompt);
            var response = await SendRequestAsync(requestBody);
            var quiz = await ParseResponseAsync(response);
            
            return quiz;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate quiz from Gemini API");
            throw new InvalidOperationException("Failed to generate quiz", ex);
        }
    }

    private object CreateRequestBody(string prompt)
    {
        return new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };
    }

    private async Task<HttpResponseMessage> SendRequestAsync(object requestBody)
    {
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var url = $"{_baseUrl}/models/{_model}:generateContent?key={_apiKey}";
        
        var response = await _httpClient.PostAsync(url, content);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Gemini API request failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
            throw new HttpRequestException($"Gemini API request failed: {response.StatusCode}");
        }

        return response;
    }

    private async Task<Quiz> ParseResponseAsync(HttpResponseMessage response)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        
        _logger.LogInformation("Raw Gemini API response: {ResponseContent}", responseContent);
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent, options);

        _logger.LogInformation("Deserialized response - Candidates count: {Count}", 
            geminiResponse?.Candidates?.Length ?? 0);

        if (geminiResponse?.Candidates == null || geminiResponse.Candidates.Length == 0)
        {
            _logger.LogError("No candidates in Gemini response. Full response: {Response}", responseContent);
            throw new InvalidOperationException("No content in Gemini response");
        }

        var content = geminiResponse.Candidates[0].Content?.Parts?[0]?.Text;
        
        _logger.LogInformation("Extracted content length: {Length}", content?.Length ?? 0);
        if (!string.IsNullOrEmpty(content))
        {
            _logger.LogInformation("Extracted content preview: {ContentPreview}", 
                content.Length > 500 ? content.Substring(0, 500) + "..." : content);
        }
        
        if (string.IsNullOrEmpty(content))
        {
            _logger.LogError("Empty content from Gemini API. Candidate structure: {@Candidate}", 
                geminiResponse.Candidates[0]);
            throw new InvalidOperationException("Empty content from Gemini API");
        }

        return ParseQuizFromJson(content);
    }

    private Quiz ParseQuizFromJson(string jsonContent)
    {
        try
        {
            var cleanJson = ExtractJsonFromContent(jsonContent);
            var quizData = JsonSerializer.Deserialize<QuizJsonData>(cleanJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (quizData == null)
            {
                throw new InvalidOperationException("Failed to deserialize quiz data");
            }

            return new Quiz
            {
                Topic = quizData.Topic ?? "Unknown Topic",
                Questions = quizData.Questions?.Select(q => new QuizQuestion
                {
                    Question = q.Question ?? string.Empty,
                    Options = q.Options ?? new List<string>(),
                    CorrectAnswerIndex = q.CorrectAnswerIndex,
                    Explanation = q.Explanation ?? string.Empty
                }).ToList() ?? new List<QuizQuestion>()
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON content: {Content}", jsonContent);
            throw new InvalidOperationException("Invalid JSON format from Gemini API", ex);
        }
    }

    private string ExtractJsonFromContent(string content)
    {
        content = content.Trim();
        
        if (content.StartsWith("```json"))
        {
            var startIndex = content.IndexOf("```json") + 7;
            var endIndex = content.LastIndexOf("```");
            if (endIndex > startIndex)
            {
                content = content.Substring(startIndex, endIndex - startIndex).Trim();
            }
        }
        else if (content.StartsWith("```"))
        {
            var startIndex = content.IndexOf("```") + 3;
            var endIndex = content.LastIndexOf("```");
            if (endIndex > startIndex)
            {
                content = content.Substring(startIndex, endIndex - startIndex).Trim();
            }
        }

        return content;
    }

    private class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public GeminiCandidate[]? Candidates { get; set; }
    }

    private class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
        
        [JsonPropertyName("finishReason")]
        public string? FinishReason { get; set; }
        
        [JsonPropertyName("avgLogprobs")]
        public double? AvgLogprobs { get; set; }
    }

    private class GeminiContent
    {
        [JsonPropertyName("parts")]
        public GeminiPart[]? Parts { get; set; }
        
        [JsonPropertyName("role")]
        public string? Role { get; set; }
    }

    private class GeminiPart
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private class QuizJsonData
    {
        public string? Topic { get; set; }
        public List<QuestionJsonData>? Questions { get; set; }
    }

    private class QuestionJsonData
    {
        public string? Question { get; set; }
        public List<string>? Options { get; set; }
        public int CorrectAnswerIndex { get; set; }
        public string? Explanation { get; set; }
    }
} 