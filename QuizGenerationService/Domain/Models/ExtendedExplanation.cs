using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Domain.Models;

public class ExtendedExplanation
{
    public QuestionId QuestionId { get; set; } = QuestionId.New();
    public string QuestionText { get; set; } = string.Empty;
    public string MainExplanation { get; set; } = string.Empty;
    public List<string> DetailedParagraphs { get; set; } = new();
    public string HistoricalContext { get; set; } = string.Empty;
    public string ContemporaryRelevance { get; set; } = string.Empty;
    public List<string> KeyConcepts { get; set; } = new();
    public List<string> RelatedPhilosophers { get; set; } = new();
    public List<SuggestedSource> SuggestedSources { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DifficultyLevel OriginalDifficulty { get; set; } = DifficultyLevel.Medium;
    public Language Language { get; set; } = Language.Polish;

    public void AddParagraph(string paragraph)
    {
        if (!string.IsNullOrWhiteSpace(paragraph))
        {
            DetailedParagraphs.Add(paragraph);
        }
    }

    public void AddKeyConcept(string concept)
    {
        if (!string.IsNullOrWhiteSpace(concept) && !KeyConcepts.Contains(concept))
        {
            KeyConcepts.Add(concept);
        }
    }

    public void AddRelatedPhilosopher(string philosopher)
    {
        if (!string.IsNullOrWhiteSpace(philosopher) && !RelatedPhilosophers.Contains(philosopher))
        {
            RelatedPhilosophers.Add(philosopher);
        }
    }

    public void AddSuggestedSource(SuggestedSource source)
    {
        SuggestedSources.Add(source);
    }

    public List<SuggestedSource> GetHighlyRelevantSources()
    {
        return SuggestedSources.Where(s => s.IsHighlyRelevant).OrderByDescending(s => s.RelevanceScore).ToList();
    }

    public List<SuggestedSource> GetSourcesByType(string type)
    {
        return SuggestedSources.Where(s => s.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
                              .OrderByDescending(s => s.RelevanceScore)
                              .ToList();
    }

    public string GetFullExplanationText()
    {
        var sections = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(MainExplanation))
            sections.Add(MainExplanation);
        
        sections.AddRange(DetailedParagraphs);
        
        if (!string.IsNullOrWhiteSpace(HistoricalContext))
            sections.Add($"Historical Context: {HistoricalContext}");
        
        if (!string.IsNullOrWhiteSpace(ContemporaryRelevance))
            sections.Add($"Contemporary Relevance: {ContemporaryRelevance}");

        return string.Join("\n\n", sections);
    }

    public int TotalParagraphs => DetailedParagraphs.Count + 
                                  (string.IsNullOrWhiteSpace(MainExplanation) ? 0 : 1) +
                                  (string.IsNullOrWhiteSpace(HistoricalContext) ? 0 : 1) +
                                  (string.IsNullOrWhiteSpace(ContemporaryRelevance) ? 0 : 1);

    public int EstimatedReadingTimeMinutes => Math.Max(1, (int)Math.Ceiling(GetFullExplanationText().Split(' ').Length / 200.0));
} 