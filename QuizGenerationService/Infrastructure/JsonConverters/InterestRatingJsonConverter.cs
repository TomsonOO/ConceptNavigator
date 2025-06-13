using System.Text.Json;
using System.Text.Json.Serialization;
using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Infrastructure.JsonConverters;

public class InterestRatingJsonConverter : JsonConverter<InterestRating>
{
    public override InterestRating Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetInt32();
        return InterestRating.Create(value);
    }

    public override void Write(Utf8JsonWriter writer, InterestRating value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
} 