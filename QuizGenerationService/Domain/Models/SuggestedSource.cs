namespace QuizGenerationService.Domain.Models;

public class SuggestedSource
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int RelevanceScore { get; set; }

    public static SuggestedSource Create(string title, string author, string type, string? description = null, int relevanceScore = 5)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));
        
        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Author cannot be empty", nameof(author));
        
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type cannot be empty", nameof(type));
        
        if (relevanceScore < 1 || relevanceScore > 10)
            throw new ArgumentException("Relevance score must be between 1 and 10", nameof(relevanceScore));

        return new SuggestedSource
        {
            Title = title,
            Author = author,
            Type = type,
            Description = description,
            RelevanceScore = relevanceScore
        };
    }

    public bool IsHighlyRelevant => RelevanceScore >= 8;
    public bool IsModeratelyRelevant => RelevanceScore >= 5 && RelevanceScore <= 7;
    public bool IsLowRelevance => RelevanceScore <= 4;
} 