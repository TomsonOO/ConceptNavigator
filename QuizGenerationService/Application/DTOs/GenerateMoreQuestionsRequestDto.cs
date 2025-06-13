namespace QuizGenerationService.Application.DTOs;

public class GenerateMoreQuestionsRequestDto
{
    public string SessionId { get; set; } = string.Empty;
    public int QuestionCount { get; set; } = 3;
    public string? QuestionType { get; set; }
    public string? Difficulty { get; set; }
    public string? Language { get; set; }
    public string? FocusArea { get; set; }
    public List<string> AdditionalInterests { get; set; } = new();
    public double MinInterestThreshold { get; set; } = 0.6;
    public bool IncludeRelatedConcepts { get; set; } = true;
    public bool AvoidRepeatedTopics { get; set; } = true;
} 