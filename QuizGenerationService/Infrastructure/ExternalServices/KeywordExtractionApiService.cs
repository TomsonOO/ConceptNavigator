using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using QuizGenerationService.Domain.Models;
using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Infrastructure.ExternalServices;

public class KeywordExtractionApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _model;
    private readonly ILogger<KeywordExtractionApiService> _logger;

    public KeywordExtractionApiService(
        HttpClient httpClient, 
        IConfiguration configuration, 
        ILogger<KeywordExtractionApiService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GEMINI_API_KEY"] ?? configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not configured");
        _baseUrl = configuration["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta";
        _model = configuration["GEMINI_MODEL"] ?? configuration["Gemini:Model"] ?? "gemini-2.0-flash";
        _logger = logger;
        
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<KeywordExtraction> ExtractKeywordsFromSessionAsync(QuizSession session)
    {
        try
        {
            var highInterestQuestions = session.GetHighInterestQuestions();
            
            if (!highInterestQuestions.Any())
            {
                _logger.LogWarning("No high interest questions found in session {SessionId}", session.Id);
                return CreateEmptyExtraction(session);
            }

            var prompt = BuildKeywordExtractionPrompt(highInterestQuestions, session.Topic, session.Language);
            
            _logger.LogDebug("Extracting keywords from {QuestionCount} high-interest questions in session {SessionId}", 
                           highInterestQuestions.Count, session.Id);

            var requestBody = CreateRequestBody(prompt);
            var response = await SendRequestAsync(requestBody);
            var extraction = await ParseKeywordResponseAsync(response, session);
            
            return extraction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract keywords from session {SessionId}", session.Id);
            return CreateEmptyExtraction(session);
        }
    }

    private KeywordExtraction CreateEmptyExtraction(QuizSession session)
    {
        return new KeywordExtraction
        {
            SourceTopic = session.Topic,
            Language = session.Language,
            TotalQuestions = session.Questions.Count,
            AverageInterestRating = session.GetAverageInterestRating()
        };
    }

    private string BuildKeywordExtractionPrompt(List<QuizQuestion> questions, string topic, Language language)
    {
        var isPolish = language.Code.ToLower() == "pl";
        
        var basePrompt = isPolish ? GetPolishExtractionPrompt() : GetEnglishExtractionPrompt();
        
        var questionsText = string.Join("\n\n", questions.Select((q, i) => 
            $"{i + 1}. {q.Question}\n   Wyjaśnienie: {q.Explanation}"));

        return $@"{basePrompt}

TEMAT GŁÓWNY: {topic}

PYTANIA Z WYSOKĄ OCENĄ ZAINTERESOWANIA:
{questionsText}

{GetJsonSchemaForKeywords(isPolish)}";
    }

    private string GetPolishExtractionPrompt()
    {
        return @"Jesteś ekspertem analizy treści filozoficznych. Twoim zadaniem jest wydobycie kluczowych słów i konceptów z pytań, które użytkownik ocenił jako szczególnie interesujące.

ZADANIE:
Przeanalizuj podane pytania filozoficzne i wydobądź:
1. Kluczowe terminy filozoficzne (najważniejsze koncepty)
2. Nazwiska filozofów i myślicieli
3. Okresy historyczne lub szkoły filozoficzne
4. Główne zagadnienia i problemy
5. Terminy techniczne i pojęcia

KRYTERIA OCENY RELEVANTNOŚCI (0.0-1.0):
- 1.0: Centralne pojęcie dla tematu
- 0.8-0.9: Bardzo ważny termin filozoficzny
- 0.6-0.7: Istotny kontekst lub powiązane pojęcie
- 0.4-0.5: Pomocny termin uzupełniający
- 0.0-0.3: Marginalny lub ogólny termin";
    }

    private string GetEnglishExtractionPrompt()
    {
        return @"You are an expert in philosophical content analysis. Your task is to extract key words and concepts from questions that the user rated as particularly interesting.

TASK:
Analyze the given philosophical questions and extract:
1. Key philosophical terms (most important concepts)
2. Names of philosophers and thinkers
3. Historical periods or philosophical schools
4. Main issues and problems
5. Technical terms and concepts

RELEVANCE SCORING CRITERIA (0.0-1.0):
- 1.0: Central concept for the topic
- 0.8-0.9: Very important philosophical term
- 0.6-0.7: Important context or related concept
- 0.4-0.5: Helpful supplementary term
- 0.0-0.3: Marginal or general term";
    }

    private string GetJsonSchemaForKeywords(bool isPolish)
    {
        return @"Odpowiedz w formacie JSON:

```json
{
  ""keywords"": [
    {
      ""keyword"": ""nazwa pojęcia"",
      ""relevanceScore"": 0.85,
      ""category"": ""concept|philosopher|historical|technical|general"",
      ""frequency"": 1
    }
  ]
}
```";
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

    private async Task<KeywordExtraction> ParseKeywordResponseAsync(HttpResponseMessage response, QuizSession session)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        
        _logger.LogDebug("Keyword extraction API response length: {Length}", responseContent.Length);
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent, options);

        if (geminiResponse?.Candidates == null || geminiResponse.Candidates.Length == 0)
        {
            _logger.LogError("No candidates in keyword extraction response");
            return CreateEmptyExtraction(session);
        }

        var content = geminiResponse.Candidates[0].Content?.Parts?[0]?.Text;
        
        if (string.IsNullOrEmpty(content))
        {
            _logger.LogError("Empty content from keyword extraction API");
            return CreateEmptyExtraction(session);
        }

        return ParseKeywordExtractionFromJson(content, session);
    }

    private KeywordExtraction ParseKeywordExtractionFromJson(string jsonContent, QuizSession session)
    {
        try
        {
            var cleanJson = ExtractJsonFromContent(jsonContent);
            var keywordData = JsonSerializer.Deserialize<KeywordExtractionJsonData>(cleanJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var extraction = new KeywordExtraction
            {
                SourceTopic = session.Topic,
                Language = session.Language,
                TotalQuestions = session.Questions.Count,
                AverageInterestRating = session.GetAverageInterestRating()
            };

            if (keywordData?.Keywords != null)
            {
                foreach (var keywordJson in keywordData.Keywords)
                {
                    if (!string.IsNullOrWhiteSpace(keywordJson.Keyword))
                    {
                        extraction.AddKeyword(
                            keywordJson.Keyword,
                            Math.Max(0.0, Math.Min(1.0, keywordJson.RelevanceScore)),
                            keywordJson.Category ?? "general"
                        );
                    }
                }
            }

            _logger.LogInformation("Extracted {KeywordCount} keywords from session {SessionId}", 
                                 extraction.Keywords.Count, session.Id);

            return extraction;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse keyword extraction JSON: {Content}", jsonContent);
            return CreateEmptyExtraction(session);
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

    private class KeywordExtractionJsonData
    {
        public List<KeywordJsonData>? Keywords { get; set; }
    }

    private class KeywordJsonData
    {
        public string? Keyword { get; set; }
        public double RelevanceScore { get; set; }
        public string? Category { get; set; }
        public int Frequency { get; set; } = 1;
    }
} 