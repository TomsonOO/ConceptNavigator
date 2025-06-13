using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Infrastructure.PromptBuilding;

public class PromptTemplateProvider
{
    private readonly Dictionary<string, PromptTemplate> _templates;

    public PromptTemplateProvider()
    {
        _templates = new Dictionary<string, PromptTemplate>
        {
            [QuestionType.Basic.Value] = CreateBasicTemplate(),
            [QuestionType.Situational.Value] = CreateSituationalTemplate()
        };
    }

    public PromptTemplate GetTemplate(QuestionType questionType)
    {
        return _templates.GetValueOrDefault(questionType.Value) ?? _templates[QuestionType.Basic.Value];
    }

    private PromptTemplate CreateBasicTemplate()
    {
        return new PromptTemplate
        {
            BasePrompt = "You are an expert philosophy educator. Create a quiz with basic questions focusing on definitions, key figures, and core concepts.",
            DifficultyInstructions = "Difficulty Level: {0} - {1}",
            LanguageInstructions = "Generate all questions and answers in {0}.",
            BookContext = "Focus specifically on concepts and ideas from the book: {0}",
            OutputFormat = @"Return ONLY a valid JSON object in this exact format:
{
  ""topic"": ""provided topic"",
  ""questions"": [
    {
      ""question"": ""question text"",
      ""options"": [""option A"", ""option B"", ""option C"", ""option D""],
      ""correctAnswerIndex"": 0,
      ""explanation"": ""2-3 sentence explanation of why this answer is correct""
    }
  ]
}"
        };
    }

    private PromptTemplate CreateSituationalTemplate()
    {
        return new PromptTemplate
        {
            BasePrompt = "You are an expert philosophy educator. Create a quiz with situational questions that present real-life scenarios requiring philosophical thinking and application of concepts.",
            DifficultyInstructions = "Difficulty Level: {0} - {1}",
            LanguageInstructions = "Generate all questions and answers in {0}.",
            BookContext = "Base scenarios on philosophical principles and ideas from the book: {0}",
            OutputFormat = @"Return ONLY a valid JSON object in this exact format:
{
  ""topic"": ""provided topic"",
  ""questions"": [
    {
      ""question"": ""situational question with scenario"",
      ""options"": [""option A"", ""option B"", ""option C"", ""option D""],
      ""correctAnswerIndex"": 0,
      ""explanation"": ""2-3 sentence explanation connecting the scenario to philosophical principles""
    }
  ]
}"
        };
    }
} 