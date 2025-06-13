using System.Text;
using QuizGenerationService.Domain.Interfaces;
using QuizGenerationService.Domain.Models;
using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Infrastructure.PromptBuilding;

public class PromptBuilder : IPromptBuilder
{
    private readonly PromptTemplateProvider _templateProvider;

    public PromptBuilder(PromptTemplateProvider templateProvider)
    {
        _templateProvider = templateProvider;
    }

    public string BuildPrompt(QuizGenerationRequest request)
    {
        var template = _templateProvider.GetTemplate(request.QuestionType);
        var builder = new StringBuilder();

        builder.AppendLine(template.BasePrompt);
        builder.AppendLine();

        if (!string.IsNullOrEmpty(request.Book))
        {
            builder.AppendLine(string.Format(template.BookContext, request.Book));
            builder.AppendLine();
        }

        builder.AppendLine(BuildDifficultySection(template, request.Difficulty));
        builder.AppendLine();

        builder.AppendLine(BuildLanguageSection(template, request.Language));
        builder.AppendLine();

        builder.AppendLine($"Topic: {request.Topic}");
        builder.AppendLine($"Number of questions: {request.QuestionCount}");
        builder.AppendLine();

        builder.AppendLine(template.OutputFormat);

        return builder.ToString();
    }

    private string BuildDifficultySection(PromptTemplate template, DifficultyLevel difficulty)
    {
        return string.Format(template.DifficultyInstructions, difficulty.Value, difficulty.Description);
    }

    private string BuildLanguageSection(PromptTemplate template, Language language)
    {
        return string.Format(template.LanguageInstructions, language.Name);
    }
} 