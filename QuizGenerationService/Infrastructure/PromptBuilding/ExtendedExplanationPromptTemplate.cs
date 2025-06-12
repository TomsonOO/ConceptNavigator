using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Infrastructure.PromptBuilding;

public class ExtendedExplanationPromptTemplate
{
    public string GeneratePrompt(
        string questionText,
        string originalExplanation,
        string topic,
        string? book,
        DifficultyLevel difficulty,
        Language language,
        List<string> userInterests,
        string? focusArea)
    {
        var languageCode = language.Code.ToLower();
        var isPolish = languageCode == "pl";
        
        var basePrompt = isPolish ? GetPolishTemplate() : GetEnglishTemplate();
        
        var contextSection = BuildContextSection(topic, book, difficulty, userInterests, focusArea, isPolish);
        var questionSection = BuildQuestionSection(questionText, originalExplanation, isPolish);
        var requirements = BuildRequirements(difficulty, isPolish);
        var jsonSchema = GetJsonSchema(isPolish);

        return $@"{basePrompt}

{contextSection}

{questionSection}

{requirements}

{jsonSchema}";
    }

    private string GetPolishTemplate()
    {
        return @"Jesteś ekspertem filozofii z doktoratem i wieloletnim doświadczeniem akademickim. Twoim zadaniem jest stworzenie szczegółowego, akademickiego wyjaśnienia pytania filozoficznego, które wykracza daleko poza podstawowe objaśnienie.

KONTEKST AKADEMICKI:
Twoja odpowiedź powinna być na poziomie uniwersyteckim, bogata w szczegóły, kontekst historyczny i współczesne zastosowania. Powinna pokazywać głębię problemu filozoficznego i jego złożoność.";
    }

    private string GetEnglishTemplate()
    {
        return @"You are a philosophy expert with a PhD and extensive academic experience. Your task is to create a detailed, academic explanation of a philosophical question that goes far beyond basic explanation.

ACADEMIC CONTEXT:
Your response should be at university level, rich in detail, historical context, and contemporary applications. It should demonstrate the depth of the philosophical problem and its complexity.";
    }

    private string BuildContextSection(string topic, string? book, DifficultyLevel difficulty, List<string> userInterests, string? focusArea, bool isPolish)
    {
        var section = isPolish ? "KONTEKST TEMATU:" : "TOPIC CONTEXT:";
        section += $"\n- {(isPolish ? "Główny temat" : "Main topic")}: {topic}";
        
        if (!string.IsNullOrWhiteSpace(book))
            section += $"\n- {(isPolish ? "Książka/Źródło" : "Book/Source")}: {book}";
        
        section += $"\n- {(isPolish ? "Poziom trudności" : "Difficulty level")}: {difficulty.Value} - {difficulty.Description}";
        
        if (userInterests.Any())
        {
            var interestsText = string.Join(", ", userInterests);
            section += $"\n- {(isPolish ? "Obszary zainteresowań użytkownika" : "User's areas of interest")}: {interestsText}";
        }
        
        if (!string.IsNullOrWhiteSpace(focusArea))
            section += $"\n- {(isPolish ? "Szczególny obszar zainteresowania" : "Specific focus area")}: {focusArea}";

        return section;
    }

    private string BuildQuestionSection(string questionText, string originalExplanation, bool isPolish)
    {
        var section = isPolish ? "PYTANIE DO ROZWINIĘCIA:" : "QUESTION TO EXPAND:";
        section += $"\n{questionText}";
        
        if (!string.IsNullOrWhiteSpace(originalExplanation))
        {
            section += $"\n\n{(isPolish ? "PODSTAWOWE WYJAŚNIENIE (do rozszerzenia)" : "BASIC EXPLANATION (to expand)")}: {originalExplanation}";
        }

        return section;
    }

    private string BuildRequirements(DifficultyLevel difficulty, bool isPolish)
    {
        var requirements = isPolish ? 
            @"WYMAGANIA DLA ROZSZERZONEGO WYJAŚNIENIA:

1. GŁÓWNE WYJAŚNIENIE: Szczegółowe, akademickie omówienie problemu (2-3 akapity)
2. DODATKOWE PARAGRAFY: 3-5 paragrafów rozwijających różne aspekty tematu
3. KONTEKST HISTORYCZNY: Rozwój historyczny koncepcji i jej ewolucja
4. WSPÓŁCZESNA RELEVANTNOŚĆ: Jak ta koncepcja odnosi się do współczesnych problemów
5. KLUCZOWE POJĘCIA: Lista najważniejszych terminów filozoficznych
6. POWIĄZANI FILOZOFOWIE: Myśliciele związani z tym tematem
7. SUGEROWANE ŹRÓDŁA: Akademickie źródła do dalszego zgłębiania (książki, artykuły)" :

            @"REQUIREMENTS FOR EXTENDED EXPLANATION:

1. MAIN EXPLANATION: Detailed, academic discussion of the problem (2-3 paragraphs)
2. ADDITIONAL PARAGRAPHS: 3-5 paragraphs developing different aspects of the topic
3. HISTORICAL CONTEXT: Historical development of the concept and its evolution
4. CONTEMPORARY RELEVANCE: How this concept relates to contemporary problems
5. KEY CONCEPTS: List of the most important philosophical terms
6. RELATED PHILOSOPHERS: Thinkers associated with this topic
7. SUGGESTED SOURCES: Academic sources for further exploration (books, articles)";

        if (difficulty.Value == "Hard")
        {
            requirements += isPolish ? 
                "\n\nDLA POZIOMU TRUDNEGO: Uwzględnij kontrowersje, różne interpretacje i najnowsze badania akademickie." :
                "\n\nFOR HARD LEVEL: Include controversies, different interpretations, and latest academic research.";
        }

        return requirements;
    }

    private string GetJsonSchema(bool isPolish)
    {
        return @"Odpowiedz w formacie JSON według tego schematu:

```json
{
  ""questionText"": ""Pytanie do wyjaśnienia"",
  ""mainExplanation"": ""Główne wyjaśnienie (2-3 akapity)"",
  ""detailedParagraphs"": [
    ""Pierwszy dodatkowy akapit z pogłębionym omówieniem"",
    ""Drugi akapit rozwijający inny aspekt"",
    ""Trzeci akapit z przykładami i zastosowaniami""
  ],
  ""historicalContext"": ""Kontekst historyczny i ewolucja koncepcji"",
  ""contemporaryRelevance"": ""Współczesna relevantność i zastosowania"",
  ""keyConcepts"": [""pojęcie1"", ""pojęcie2"", ""pojęcie3""],
  ""relatedPhilosophers"": [""Filozof 1"", ""Filozof 2"", ""Filozof 3""],
  ""suggestedSources"": [
    {
      ""title"": ""Tytuł książki/artykułu"",
      ""author"": ""Autor"",
      ""type"": ""book|article|encyclopedia"",
      ""description"": ""Krótki opis dlaczego to źródło jest istotne"",
      ""relevanceScore"": 8
    }
  ]
}
```";
    }
} 