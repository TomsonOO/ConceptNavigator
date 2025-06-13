using QuizGenerationService.Domain.Models;
using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Infrastructure.PromptBuilding;

public class AdaptivePromptBuilder
{
    public string BuildAdaptivePrompt(AdaptiveQuizRequest request)
    {
        var isPolish = request.Language.Code.ToLower() == "pl";
        
        var basePrompt = isPolish ? GetPolishAdaptiveTemplate() : GetEnglishAdaptiveTemplate();
        var contextSection = BuildContextSection(request, isPolish);
        var personalizationSection = BuildPersonalizationSection(request, isPolish);
        var requirements = BuildRequirements(request, isPolish);
        var jsonSchema = GetJsonSchema(isPolish);

        return $@"{basePrompt}

{contextSection}

{personalizationSection}

{requirements}

{jsonSchema}";
    }

    private string GetPolishAdaptiveTemplate()
    {
        return @"Jesteś ekspertem filozofii specjalizującym się w tworzeniu spersonalizowanych pytań. Twoim zadaniem jest stworzenie pytań filozoficznych, które będą szczególnie interesujące dla użytkownika na podstawie jego wcześniejszych preferencji.

ADAPTACYJNE PODEJŚCIE:
Analizujesz preferencje użytkownika i tworzysz pytania, które:
- Nawiązują do pojęć, które wcześniej ocenił jako interesujące
- Rozwijają tematy, którymi się fascynuje
- Wprowadzają nowe perspektywy na znane mu zagadnienia
- Są dostosowane do jego poziomu zainteresowania";
    }

    private string GetEnglishAdaptiveTemplate()
    {
        return @"You are a philosophy expert specializing in creating personalized questions. Your task is to create philosophical questions that will be particularly interesting to the user based on their previous preferences.

ADAPTIVE APPROACH:
You analyze user preferences and create questions that:
- Reference concepts they previously rated as interesting
- Develop topics they are fascinated by
- Introduce new perspectives on familiar issues
- Are tailored to their level of interest";
    }

    private string BuildContextSection(AdaptiveQuizRequest request, bool isPolish)
    {
        var section = isPolish ? "KONTEKST TEMATU:" : "TOPIC CONTEXT:";
        section += $"\n- {(isPolish ? "Główny temat" : "Main topic")}: {request.Topic}";
        
        if (!string.IsNullOrWhiteSpace(request.Book))
            section += $"\n- {(isPolish ? "Książka/Źródło" : "Book/Source")}: {request.Book}";
        
        section += $"\n- {(isPolish ? "Typ pytań" : "Question type")}: {request.QuestionType.Value}";
        section += $"\n- {(isPolish ? "Poziom trudności" : "Difficulty level")}: {request.Difficulty.Value}";
        section += $"\n- {(isPolish ? "Liczba pytań" : "Number of questions")}: {request.QuestionCount}";

        return section;
    }

    private string BuildPersonalizationSection(AdaptiveQuizRequest request, bool isPolish)
    {
        var section = isPolish ? "PERSONALIZACJA NA PODSTAWIE PREFERENCJI:" : "PERSONALIZATION BASED ON PREFERENCES:";
        
        if (request.PriorityKeywords.Any())
        {
            var keywordsByCategory = request.GetCategorizedKeywords();
            section += isPolish ? "\n\nKLUCZOWE POJĘCIA Z WYSOKĄ OCENĄ:" : "\n\nHIGH-RATED KEY CONCEPTS:";
            
            foreach (var category in keywordsByCategory.Keys)
            {
                var categoryName = GetCategoryName(category, isPolish);
                var keywords = string.Join(", ", keywordsByCategory[category]);
                section += $"\n- {categoryName}: {keywords}";
            }
        }

        if (request.UserInterests.Any())
        {
            var interestsText = string.Join(", ", request.UserInterests);
            section += $"\n\n{(isPolish ? "DODATKOWE ZAINTERESOWANIA" : "ADDITIONAL INTERESTS")}: {interestsText}";
        }

        if (!string.IsNullOrWhiteSpace(request.FocusArea))
        {
            section += $"\n\n{(isPolish ? "SZCZEGÓLNY OBSZAR ZAINTERESOWANIA" : "SPECIFIC FOCUS AREA")}: {request.FocusArea}";
        }

        if (request.AvoidRepeatedTopics)
        {
            section += isPolish ? 
                "\n\n⚠️ UNIKAJ powtarzania dokładnie tych samych pytań lub bardzo podobnych zagadnień." :
                "\n\n⚠️ AVOID repeating exactly the same questions or very similar topics.";
        }

        return section;
    }

    private string GetCategoryName(string category, bool isPolish)
    {
        return category.ToLower() switch
        {
            "concept" => isPolish ? "Pojęcia filozoficzne" : "Philosophical concepts",
            "philosopher" => isPolish ? "Filozofowie" : "Philosophers",
            "historical" => isPolish ? "Kontekst historyczny" : "Historical context",
            "technical" => isPolish ? "Terminy techniczne" : "Technical terms",
            _ => isPolish ? "Ogólne" : "General"
        };
    }

    private string BuildRequirements(AdaptiveQuizRequest request, bool isPolish)
    {
        var requirements = isPolish ?
            @"WYMAGANIA DLA ADAPTACYJNYCH PYTAŃ:

1. PERSONALIZACJA: Każde pytanie powinno nawiązywać do przynajmniej jednego kluczowego pojęcia lub zainteresowania użytkownika
2. PROGRESJA: Pytania powinny rozwijać tematy, które użytkownik już uznał za interesujące
3. NOWOŚĆ: Wprowadzaj nowe perspektywy lub głębsze aspekty znanych zagadnień
4. SPÓJNOŚĆ: Zachowaj logiczny przepływ między pytaniami
5. WYJAŚNIENIA: Każde wyjaśnienie powinno łączyć odpowiedź z szerszym kontekstem filozoficznym" :

            @"REQUIREMENTS FOR ADAPTIVE QUESTIONS:

1. PERSONALIZATION: Each question should reference at least one key concept or user interest
2. PROGRESSION: Questions should develop topics the user already found interesting
3. NOVELTY: Introduce new perspectives or deeper aspects of familiar issues
4. COHERENCE: Maintain logical flow between questions
5. EXPLANATIONS: Each explanation should connect the answer to broader philosophical context";

        if (request.IncludeRelatedConcepts)
        {
            requirements += isPolish ?
                "\n6. POWIĄZANIA: Włączaj pojęcia powiązane z kluczowymi terminami użytkownika" :
                "\n6. CONNECTIONS: Include concepts related to the user's key terms";
        }

        return requirements;
    }

    private string GetJsonSchema(bool isPolish)
    {
        return @"Odpowiedz w formacie JSON według tego schematu:

```json
{
  ""topic"": ""Nazwa tematu"",
  ""questions"": [
    {
      ""question"": ""Treść pytania nawiązującego do zainteresowań użytkownika"",
      ""options"": [""Opcja A"", ""Opcja B"", ""Opcja C"", ""Opcja D""],
      ""correctAnswerIndex"": 1,
      ""explanation"": ""Wyjaśnienie łączące odpowiedź z kluczowymi pojęciami użytkownika"",
      ""keywords"": [""słowo1"", ""słowo2"", ""słowo3""]
    }
  ]
}
```

WAŻNE: Pole 'keywords' powinno zawierać pojęcia z tego pytania, które mogą być interesujące dla użytkownika.";
    }
} 