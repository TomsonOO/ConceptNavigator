using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Domain.Models;

public class AdaptiveQuizRequest
{
    public SessionId SessionId { get; set; } = SessionId.New();
    public string Topic { get; set; } = string.Empty;
    public string? Book { get; set; }
    public QuestionType QuestionType { get; set; } = QuestionType.Basic;
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
    public Language Language { get; set; } = Language.Polish;
    public int QuestionCount { get; set; } = 3;
    
    public List<ExtractedKeyword> PriorityKeywords { get; set; } = new();
    public List<string> UserInterests { get; set; } = new();
    public double MinInterestThreshold { get; set; } = 0.6;
    public string? FocusArea { get; set; }
    public bool IncludeRelatedConcepts { get; set; } = true;
    public bool AvoidRepeatedTopics { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public void AddPriorityKeyword(ExtractedKeyword keyword)
    {
        if (keyword.IsHighPriority && !PriorityKeywords.Any(k => 
            k.Keyword.Equals(keyword.Keyword, StringComparison.OrdinalIgnoreCase)))
        {
            PriorityKeywords.Add(keyword);
        }
    }

    public void AddUserInterest(string interest)
    {
        if (!string.IsNullOrWhiteSpace(interest) && !UserInterests.Contains(interest))
        {
            UserInterests.Add(interest);
        }
    }

    public List<string> GetTopKeywords(int maxCount = 5)
    {
        return PriorityKeywords
            .OrderByDescending(k => k.RelevanceScore)
            .Take(maxCount)
            .Select(k => k.Keyword)
            .ToList();
    }

    public Dictionary<string, List<string>> GetCategorizedKeywords()
    {
        return PriorityKeywords
            .GroupBy(k => k.Category)
            .ToDictionary(
                g => g.Key, 
                g => g.OrderByDescending(k => k.RelevanceScore).Select(k => k.Keyword).ToList());
    }

    public bool ShouldFocusOnKeywords => PriorityKeywords.Any();
    public bool HasUserPreferences => UserInterests.Any() || !string.IsNullOrWhiteSpace(FocusArea);
} 