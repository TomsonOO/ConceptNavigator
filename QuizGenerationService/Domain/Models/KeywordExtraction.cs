using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Domain.Models;

public class KeywordExtraction
{
    public List<ExtractedKeyword> Keywords { get; set; } = new();
    public string SourceTopic { get; set; } = string.Empty;
    public Language Language { get; set; } = Language.Polish;
    public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;
    public int TotalQuestions { get; set; }
    public double AverageInterestRating { get; set; }

    public void AddKeyword(string keyword, double relevanceScore, string category = "general")
    {
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var existingKeyword = Keywords.FirstOrDefault(k => 
                k.Keyword.Equals(keyword, StringComparison.OrdinalIgnoreCase));
            
            if (existingKeyword != null)
            {
                existingKeyword.RelevanceScore = Math.Max(existingKeyword.RelevanceScore, relevanceScore);
                existingKeyword.Frequency++;
            }
            else
            {
                Keywords.Add(new ExtractedKeyword
                {
                    Keyword = keyword,
                    RelevanceScore = relevanceScore,
                    Category = category,
                    Frequency = 1
                });
            }
        }
    }

    public List<ExtractedKeyword> GetHighPriorityKeywords(int maxCount = 10)
    {
        return Keywords
            .Where(k => k.IsHighPriority)
            .OrderByDescending(k => k.RelevanceScore)
            .ThenByDescending(k => k.Frequency)
            .Take(maxCount)
            .ToList();
    }

    public List<ExtractedKeyword> GetKeywordsByCategory(string category)
    {
        return Keywords
            .Where(k => k.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(k => k.RelevanceScore)
            .ToList();
    }

    public Dictionary<string, List<ExtractedKeyword>> GetCategorizedKeywords()
    {
        return Keywords
            .GroupBy(k => k.Category)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(k => k.RelevanceScore).ToList());
    }
}

public class ExtractedKeyword
{
    public string Keyword { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
    public string Category { get; set; } = "general";
    public int Frequency { get; set; } = 1;
    public DateTime FirstSeen { get; set; } = DateTime.UtcNow;

    public bool IsHighPriority => RelevanceScore >= 0.7;
    public bool IsMediumPriority => RelevanceScore >= 0.4 && RelevanceScore < 0.7;
    public bool IsLowPriority => RelevanceScore < 0.4;
} 