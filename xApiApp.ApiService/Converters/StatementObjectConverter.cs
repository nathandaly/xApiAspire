using System.Text.Json;
using System.Text.Json.Serialization;
using xApiApp.ApiService.Models;

namespace xApiApp.ApiService.Converters;

public class StatementObjectConverter : JsonConverter<StatementObject>
{
    public override StatementObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var doc = JsonDocument.ParseValue(ref reader);
        try
        {
            var root = doc.RootElement;
            
            if (!root.TryGetProperty("objectType", out var objectTypeElement))
            {
                // Default to Activity if objectType is not specified
                return JsonSerializer.Deserialize<Activity>(root.GetRawText(), options);
            }

            var objectType = objectTypeElement.GetString();
            return objectType switch
            {
                "Activity" => JsonSerializer.Deserialize<Activity>(root.GetRawText(), options),
                "Agent" => JsonSerializer.Deserialize<AgentAsObject>(root.GetRawText(), options),
                "Group" => JsonSerializer.Deserialize<GroupAsObject>(root.GetRawText(), options),
                "StatementRef" => JsonSerializer.Deserialize<StatementRef>(root.GetRawText(), options),
                "SubStatement" => JsonSerializer.Deserialize<SubStatement>(root.GetRawText(), options),
                _ => JsonSerializer.Deserialize<Activity>(root.GetRawText(), options) // Default to Activity
            };
        }
        finally
        {
            doc.Dispose();
        }
    }

    public override void Write(Utf8JsonWriter writer, StatementObject value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}

