using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using QuizGenerationService.Domain.Models;
using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Infrastructure.ExternalServices;

public class ConceptMapApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _model;
    private readonly ILogger<ConceptMapApiService> _logger;

    public ConceptMapApiService(
        HttpClient httpClient, 
        IConfiguration configuration, 
        ILogger<ConceptMapApiService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GEMINI_API_KEY"] ?? configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not configured");
        _baseUrl = configuration["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta";
        _model = configuration["GEMINI_MODEL"] ?? configuration["Gemini:Model"] ?? "gemini-2.0-flash";
        _logger = logger;
        
        _httpClient.Timeout = TimeSpan.FromSeconds(60);
    }

    public async Task<ConceptMap> GenerateConceptMapAsync(
        string topic,
        Language language,
        List<string> focusKeywords,
        string? book = null,
        SessionId? sessionId = null,
        int maxNodes = 15,
        bool includePhilosophers = true,
        bool includeHistoricalContext = true)
    {
        try
        {
            var prompt = BuildConceptMapPrompt(topic, language, focusKeywords, book, maxNodes, includePhilosophers, includeHistoricalContext);
            
            _logger.LogDebug("Generating concept map for topic: {Topic}, keywords: {KeywordCount}, max nodes: {MaxNodes}", 
                           topic, focusKeywords.Count, maxNodes);

            var requestBody = CreateRequestBody(prompt);
            var response = await SendRequestAsync(requestBody);
            var conceptMap = await ParseConceptMapResponseAsync(response, topic, language, book, sessionId);
            
            return conceptMap;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate concept map for topic: {Topic}", topic);
            throw new InvalidOperationException("Failed to generate concept map", ex);
        }
    }

    private string BuildConceptMapPrompt(
        string topic, 
        Language language, 
        List<string> focusKeywords, 
        string? book, 
        int maxNodes,
        bool includePhilosophers,
        bool includeHistoricalContext)
    {
        var isPolish = language.Code.ToLower() == "pl";
        
        var basePrompt = isPolish ? GetPolishConceptMapPrompt() : GetEnglishConceptMapPrompt();
        var contextSection = BuildContextSection(topic, book, focusKeywords, isPolish);
        var requirements = BuildRequirements(maxNodes, includePhilosophers, includeHistoricalContext, isPolish);
        var jsonSchema = GetJsonSchemaForConceptMap(isPolish);

        return $@"{basePrompt}

{contextSection}

{requirements}

{jsonSchema}";
    }

    private string GetPolishConceptMapPrompt()
    {
        return @"Jesteś ekspertem filozofii specjalizującym się w tworzeniu map konceptualnych. Twoim zadaniem jest stworzenie strukturalnej mapy konceptualnej pokazującej powiązania między kluczowymi pojęciami filozoficznymi.

ZADANIE TWORZENIA MAPY KONCEPTUALNEJ:
Tworzysz wizualną reprezentację pojęć filozoficznych i ich wzajemnych relacji, która pomoże w zrozumieniu struktury tematu i połączeń między różnymi ideami.";
    }

    private string GetEnglishConceptMapPrompt()
    {
        return @"You are a philosophy expert specializing in creating concept maps. Your task is to create a structural concept map showing connections between key philosophical concepts.

CONCEPT MAP CREATION TASK:
You create a visual representation of philosophical concepts and their relationships that helps understand the topic structure and connections between different ideas.";
    }

    private string BuildContextSection(string topic, string? book, List<string> focusKeywords, bool isPolish)
    {
        var section = isPolish ? "KONTEKST MAPY KONCEPTUALNEJ:" : "CONCEPT MAP CONTEXT:";
        section += $"\n- {(isPolish ? "Główny temat" : "Main topic")}: {topic}";
        
        if (!string.IsNullOrWhiteSpace(book))
            section += $"\n- {(isPolish ? "Książka/Źródło" : "Book/Source")}: {book}";
        
        if (focusKeywords.Any())
        {
            var keywordsText = string.Join(", ", focusKeywords);
            section += $"\n- {(isPolish ? "Kluczowe pojęcia do uwzględnienia" : "Key concepts to include")}: {keywordsText}";
        }

        return section;
    }

    private string BuildRequirements(int maxNodes, bool includePhilosophers, bool includeHistoricalContext, bool isPolish)
    {
        var requirements = isPolish ?
            $@"WYMAGANIA DLA MAPY KONCEPTUALNEJ:

1. LICZBA WĘZŁÓW: Maksymalnie {maxNodes} głównych pojęć
2. TYPY POJĘĆ: Uwzględnij różne typy - podstawowe koncepty, problemy filozoficzne, metody
3. RELACJE: Określ jasne związki między pojęciami (podobieństwo, przeciwieństwo, część-całość, wpływ)
4. WAŻNOŚĆ: Oceń znaczenie każdego pojęcia (0.0-1.0)
5. STRUKTURA: Zachowaj logiczną hierarchię i grupowanie podobnych pojęć" :

            $@"CONCEPT MAP REQUIREMENTS:

1. NODE COUNT: Maximum {maxNodes} main concepts
2. CONCEPT TYPES: Include different types - core concepts, philosophical problems, methods
3. RELATIONSHIPS: Define clear connections between concepts (similarity, opposition, part-whole, influence)
4. IMPORTANCE: Rate the importance of each concept (0.0-1.0)
5. STRUCTURE: Maintain logical hierarchy and grouping of similar concepts";

        if (includePhilosophers)
        {
            requirements += isPolish ?
                "\n6. FILOZOFOWIE: Uwzględnij kluczowych myślicieli związanych z tematem" :
                "\n6. PHILOSOPHERS: Include key thinkers related to the topic";
        }

        if (includeHistoricalContext)
        {
            requirements += isPolish ?
                "\n7. KONTEKST HISTORYCZNY: Dodaj informacje o okresach i szkołach filozoficznych" :
                "\n7. HISTORICAL CONTEXT: Add information about periods and philosophical schools";
        }

        return requirements;
    }

    private string GetJsonSchemaForConceptMap(bool isPolish)
    {
        return @"Odpowiedz w formacie JSON według tego schematu:

```json
{
  ""nodes"": [
    {
      ""name"": ""Nazwa pojęcia"",
      ""description"": ""Opis pojęcia (2-3 zdania)"",
      ""type"": ""CoreConcept|PhilosophicalProblem|Philosopher|School|HistoricalPeriod|Method|General"",
      ""importanceScore"": 0.85,
      ""relatedPhilosopher"": ""Imię filozofa (opcjonalne)"",
      ""historicalPeriod"": ""Okres historyczny (opcjonalne)"",
      ""keywords"": [""słowo1"", ""słowo2""]
    }
  ],
  ""relationships"": [
    {
      ""sourceNode"": ""Nazwa pojęcia źródłowego"",
      ""targetNode"": ""Nazwa pojęcia docelowego"",
      ""type"": ""Related|PartOf|InstanceOf|Similar|Opposite|Influences|DevelopedBy|Precedes"",
      ""description"": ""Opis relacji"",
      ""strength"": 0.7
    }
  ]
}
```

WAŻNE: 
- Każdy węzeł musi mieć unikalną nazwę
- Relacje mogą odwoływać się tylko do istniejących węzłów
- Wszystkie nazwy muszą być dokładnie takie same w nodes i relationships";
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

    private async Task<ConceptMap> ParseConceptMapResponseAsync(HttpResponseMessage response, string topic, Language language, string? book, SessionId? sessionId)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        
        _logger.LogDebug("Concept map API response length: {Length}", responseContent.Length);
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent, options);

        if (geminiResponse?.Candidates == null || geminiResponse.Candidates.Length == 0)
        {
            _logger.LogError("No candidates in concept map response");
            throw new InvalidOperationException("No content in Gemini response");
        }

        var content = geminiResponse.Candidates[0].Content?.Parts?[0]?.Text;
        
        if (string.IsNullOrEmpty(content))
        {
            _logger.LogError("Empty content from concept map API");
            throw new InvalidOperationException("Empty content from Gemini API");
        }

        return ParseConceptMapFromJson(content, topic, language, book, sessionId);
    }

    private ConceptMap ParseConceptMapFromJson(string jsonContent, string topic, Language language, string? book, SessionId? sessionId)
    {
        try
        {
            var cleanJson = ExtractJsonFromContent(jsonContent);
            var mapData = JsonSerializer.Deserialize<ConceptMapJsonData>(cleanJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (mapData == null)
            {
                throw new InvalidOperationException("Failed to deserialize concept map data");
            }

            var conceptMap = ConceptMap.Create(topic, language, book, sessionId);

            var nodeIdMapping = new Dictionary<string, string>();

            if (mapData.Nodes != null)
            {
                foreach (var nodeData in mapData.Nodes)
                {
                    if (!string.IsNullOrWhiteSpace(nodeData.Name))
                    {
                        try
                        {
                            var conceptType = ConceptType.FromString(nodeData.Type ?? "General");
                            var node = ConceptNode.Create(
                                nodeData.Name,
                                nodeData.Description ?? "",
                                conceptType,
                                Math.Max(0.0, Math.Min(1.0, nodeData.ImportanceScore))
                            );

                            if (!string.IsNullOrWhiteSpace(nodeData.RelatedPhilosopher))
                                node.SetPhilosopher(nodeData.RelatedPhilosopher);

                            if (!string.IsNullOrWhiteSpace(nodeData.HistoricalPeriod))
                                node.SetHistoricalPeriod(nodeData.HistoricalPeriod);

                            if (nodeData.Keywords != null)
                            {
                                foreach (var keyword in nodeData.Keywords)
                                {
                                    node.AddKeyword(keyword);
                                }
                            }

                            conceptMap.AddNode(node);
                            nodeIdMapping[nodeData.Name] = node.Id;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to create concept node: {Name}", nodeData.Name);
                        }
                    }
                }
            }

            if (mapData.Relationships != null)
            {
                foreach (var relData in mapData.Relationships)
                {
                    if (!string.IsNullOrWhiteSpace(relData.SourceNode) && 
                        !string.IsNullOrWhiteSpace(relData.TargetNode) &&
                        nodeIdMapping.ContainsKey(relData.SourceNode) &&
                        nodeIdMapping.ContainsKey(relData.TargetNode))
                    {
                        try
                        {
                            var relationshipType = RelationshipType.FromString(relData.Type ?? "Related");
                            var relationship = ConceptRelationship.Create(
                                nodeIdMapping[relData.SourceNode],
                                nodeIdMapping[relData.TargetNode],
                                relationshipType,
                                relData.Description ?? "",
                                Math.Max(0.0, Math.Min(1.0, relData.Strength))
                            );

                            conceptMap.AddRelationship(relationship);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to create relationship: {Source} -> {Target}", relData.SourceNode, relData.TargetNode);
                        }
                    }
                }
            }

            _logger.LogInformation("Generated concept map with {NodeCount} nodes and {RelationshipCount} relationships for topic: {Topic}", 
                                 conceptMap.TotalNodes, conceptMap.TotalRelationships, topic);

            return conceptMap;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse concept map JSON: {Content}", jsonContent);
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

    private class ConceptMapJsonData
    {
        public List<NodeJsonData>? Nodes { get; set; }
        public List<RelationshipJsonData>? Relationships { get; set; }
    }

    private class NodeJsonData
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public double ImportanceScore { get; set; } = 0.5;
        public string? RelatedPhilosopher { get; set; }
        public string? HistoricalPeriod { get; set; }
        public List<string>? Keywords { get; set; }
    }

    private class RelationshipJsonData
    {
        public string? SourceNode { get; set; }
        public string? TargetNode { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public double Strength { get; set; } = 0.5;
    }
} 