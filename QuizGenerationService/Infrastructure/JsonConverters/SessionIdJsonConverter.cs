using System.Text.Json;
using System.Text.Json.Serialization;
using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Infrastructure.SessionStorage;

public class SessionIdJsonConverter : JsonConverter<SessionId>
{
    public override SessionId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value != null ? SessionId.FromString(value) : SessionId.New();
    }

    public override void Write(Utf8JsonWriter writer, SessionId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
} 
