using System.Text.Json;
using System.Text.Json.Serialization;
using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Infrastructure.SessionStorage;

public class QuestionIdJsonConverter : JsonConverter<QuestionId>
{
    public override QuestionId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value != null ? QuestionId.FromString(value) : QuestionId.New();
    }

    public override void Write(Utf8JsonWriter writer, QuestionId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
} 
