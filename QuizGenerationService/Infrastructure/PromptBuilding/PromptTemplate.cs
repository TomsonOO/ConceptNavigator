namespace QuizGenerationService.Infrastructure.PromptBuilding;

public class PromptTemplate
{
    public string BasePrompt { get; set; } = string.Empty;
    public string DifficultyInstructions { get; set; } = string.Empty;
    public string LanguageInstructions { get; set; } = string.Empty;
    public string BookContext { get; set; } = string.Empty;
    public string OutputFormat { get; set; } = string.Empty;
} 