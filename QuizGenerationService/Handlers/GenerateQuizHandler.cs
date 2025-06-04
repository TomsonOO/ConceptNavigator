using System.Text;
using System.Text.Json;
using QuizGenerationService.Models;
using QuizGenerationService.Queries;

namespace QuizGenerationService.Handlers;

public class GenerateQuizHandler
{
    private readonly HttpClient _httpClient;
    private readonly string _geminiApiKey;
    private readonly string _geminiModel;

    public GenerateQuizHandler(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _geminiApiKey = configuration["GEMINI_API_KEY"] ?? "";
        _geminiModel = configuration["GEMINI_MODEL"] ?? "gemini-2.0-flash";
    }

    public async Task<Quiz> Handle(GenerateQuizQuery query)
    {
        var prompt = $"Generate a {query.Difficulty} level quiz about {query.Topic} with {query.QuestionCount} multiple choice questions. Return ONLY valid JSON in this exact format: {{\"questions\":[{{\"question\":\"...\",\"options\":[\"A\",\"B\",\"C\",\"D\"],\"correctAnswerIndex\":0}}]}}";

        var requestBody = new
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

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_geminiModel}:generateContent?key={_geminiApiKey}";
        var response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Gemini API error: {response.StatusCode} - {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        
        try
        {
            var geminiResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            if (!geminiResponse.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            {
                throw new Exception("No candidates returned from Gemini API");
            }

            var candidate = candidates[0];
            if (!candidate.TryGetProperty("content", out var contentElement))
            {
                throw new Exception("No content in Gemini response");
            }

            if (!contentElement.TryGetProperty("parts", out var parts) || parts.GetArrayLength() == 0)
            {
                throw new Exception("No parts in Gemini response content");
            }

            var textPart = parts[0];
            if (!textPart.TryGetProperty("text", out var textElement))
            {
                throw new Exception("No text in Gemini response part");
            }

            var geminiText = textElement.GetString() ?? "";
            
            var startIndex = geminiText.IndexOf('{');
            var endIndex = geminiText.LastIndexOf('}');
            
            if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
            {
                throw new Exception($"No valid JSON found in response: {geminiText}");
            }

            var jsonText = geminiText.Substring(startIndex, endIndex - startIndex + 1);
            var quizData = JsonSerializer.Deserialize<JsonElement>(jsonText);

            var quiz = new Quiz
            {
                Topic = query.Topic,
                Questions = new List<QuizQuestion>()
            };

            if (quizData.TryGetProperty("questions", out var questionsArray))
            {
                foreach (var questionElement in questionsArray.EnumerateArray())
                {
                    var question = new QuizQuestion();
                    
                    if (questionElement.TryGetProperty("question", out var questionText))
                    {
                        question.Question = questionText.GetString() ?? "";
                    }
                    
                    if (questionElement.TryGetProperty("options", out var optionsArray))
                    {
                        question.Options = optionsArray.EnumerateArray()
                            .Select(opt => opt.GetString() ?? "")
                            .ToList();
                    }
                    
                    if (questionElement.TryGetProperty("correctAnswerIndex", out var correctAnswer))
                    {
                        question.CorrectAnswerIndex = correctAnswer.GetInt32();
                    }
                    
                    quiz.Questions.Add(question);
                }
            }

            return quiz;
        }
        catch (JsonException ex)
        {
            throw new Exception($"Failed to parse JSON response: {ex.Message}. Response: {responseContent}");
        }
    }
}

