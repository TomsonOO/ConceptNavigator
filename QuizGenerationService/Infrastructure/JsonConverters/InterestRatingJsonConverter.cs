using System.Text.Json;
using System.Text.Json.Serialization;
using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Infrastructure.SessionStorage;

public class InterestRatingJsonConverter : JsonConverter<InterestRating?>
{
    public override InterestRating? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        var value = reader.GetInt32();
        return InterestRating.FromNullableInt(value);
    }

    public override void Write(Utf8JsonWriter writer, InterestRating? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteNumberValue(value.Value);
        }
    }
} 