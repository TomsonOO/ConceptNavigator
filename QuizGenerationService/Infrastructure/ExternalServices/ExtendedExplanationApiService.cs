using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using QuizGenerationService.Domain.Models;
using QuizGenerationService.Domain.ValueObjects;
using QuizGenerationService.Infrastructure.PromptBuilding;

namespace QuizGenerationService.Infrastructure.ExternalServices;

public class ExtendedExplanationApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _model;
    private readonly ILogger<ExtendedExplanationApiService> _logger;
    private readonly ExtendedExplanationPromptTemplate _promptTemplate;

    public ExtendedExplanationApiService(
        HttpClient httpClient, 
        IConfiguration configuration, 
        ILogger<ExtendedExplanationApiService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GEMINI_API_KEY"] ?? configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not configured");
        _baseUrl = configuration["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta";
        _model = configuration["GEMINI_MODEL"] ?? configuration["Gemini:Model"] ?? "gemini-2.0-flash";
        _logger = logger;
        _promptTemplate = new ExtendedExplanationPromptTemplate();
        
        _httpClient.Timeout = TimeSpan.FromSeconds(45);
    }

    public async Task<ExtendedExplanation> GenerateExtendedExplanationAsync(
        string questionText,
        string originalExplanation,
        string topic,
        string? book,
        DifficultyLevel difficulty,
        Language language,
        List<string> userInterests,
        string? focusArea)
    {
        try
        {
            var prompt = _promptTemplate.GeneratePrompt(
                questionText, originalExplanation, topic, book, 
                difficulty, language, userInterests, focusArea);

            _logger.LogDebug("Generated extended explanation prompt for question: {Question}", questionText);

            var requestBody = CreateRequestBody(prompt);
            var response = await SendRequestAsync(requestBody);
            var explanation = await ParseResponseAsync(response, difficulty, language);
            
            return explanation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate extended explanation for question: {Question}", questionText);
            throw new InvalidOperationException("Failed to generate extended explanation", ex);
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

    private async Task<ExtendedExplanation> ParseResponseAsync(HttpResponseMessage response, DifficultyLevel difficulty, Language language)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        
        _logger.LogDebug("Extended explanation API response length: {Length}", responseContent.Length);
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent, options);

        if (geminiResponse?.Candidates == null || geminiResponse.Candidates.Length == 0)
        {
            _logger.LogError("No candidates in extended explanation response");
            throw new InvalidOperationException("No content in Gemini response");
        }

        var content = geminiResponse.Candidates[0].Content?.Parts?[0]?.Text;
        
        if (string.IsNullOrEmpty(content))
        {
            _logger.LogError("Empty content from extended explanation API");
            throw new InvalidOperationException("Empty content from Gemini API");
        }

        return ParseExtendedExplanationFromJson(content, difficulty, language);
    }

    private ExtendedExplanation ParseExtendedExplanationFromJson(string jsonContent, DifficultyLevel difficulty, Language language)
    {
        try
        {
            var cleanJson = ExtractJsonFromContent(jsonContent);
            var explanationData = JsonSerializer.Deserialize<ExtendedExplanationJsonData>(cleanJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (explanationData == null)
            {
                throw new InvalidOperationException("Failed to deserialize extended explanation data");
            }

            var explanation = new ExtendedExplanation
            {
                QuestionText = explanationData.QuestionText ?? string.Empty,
                MainExplanation = explanationData.MainExplanation ?? string.Empty,
                DetailedParagraphs = explanationData.DetailedParagraphs ?? new List<string>(),
                HistoricalContext = explanationData.HistoricalContext ?? string.Empty,
                ContemporaryRelevance = explanationData.ContemporaryRelevance ?? string.Empty,
                KeyConcepts = explanationData.KeyConcepts ?? new List<string>(),
                RelatedPhilosophers = explanationData.RelatedPhilosophers ?? new List<string>(),
                OriginalDifficulty = difficulty,
                Language = language
            };

            if (explanationData.SuggestedSources != null)
            {
                foreach (var sourceData in explanationData.SuggestedSources)
                {
                    try
                    {
                        var source = SuggestedSource.Create(
                            sourceData.Title ?? "Unknown Title",
                            sourceData.Author ?? "Unknown Author",
                            sourceData.Type ?? "book",
                            sourceData.Description,
                            Math.Max(1, Math.Min(10, sourceData.RelevanceScore))
                        );
                        explanation.AddSuggestedSource(source);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to create suggested source: {Title}", sourceData.Title);
                    }
                }
            }

            return explanation;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse extended explanation JSON: {Content}", jsonContent);
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
    }

    private class GeminiContent
    {
        [JsonPropertyName("parts")]
        public GeminiPart[]? Parts { get; set; }
    }

    private class GeminiPart
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private class ExtendedExplanationJsonData
    {
        public string? QuestionText { get; set; }
        public string? MainExplanation { get; set; }
        public List<string>? DetailedParagraphs { get; set; }
        public string? HistoricalContext { get; set; }
        public string? ContemporaryRelevance { get; set; }
        public List<string>? KeyConcepts { get; set; }
        public List<string>? RelatedPhilosophers { get; set; }
        public List<SuggestedSourceJsonData>? SuggestedSources { get; set; }
    }

    private class SuggestedSourceJsonData
    {
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public int RelevanceScore { get; set; } = 5;
    }
} 