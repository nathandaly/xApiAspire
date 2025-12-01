using System.Text.Json;
using System.Text.Json.Serialization;
using xApiApp.ApiService.Models;

namespace xApiApp.ApiService.Converters;

public class ActorConverter : JsonConverter<Actor>
{
    public override Actor? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var doc = JsonDocument.ParseValue(ref reader);
        try
        {
            var root = doc.RootElement;
            
            if (!root.TryGetProperty("objectType", out var objectTypeElement))
            {
                // Default to Agent if objectType is not specified
                return JsonSerializer.Deserialize<Agent>(root.GetRawText(), options);
            }

            var objectType = objectTypeElement.GetString();
            return objectType switch
            {
                "Agent" => JsonSerializer.Deserialize<Agent>(root.GetRawText(), options),
                "Group" => JsonSerializer.Deserialize<Group>(root.GetRawText(), options),
                _ => JsonSerializer.Deserialize<Agent>(root.GetRawText(), options) // Default to Agent
            };
        }
        finally
        {
            doc.Dispose();
        }
    }

    public override void Write(Utf8JsonWriter writer, Actor value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}

