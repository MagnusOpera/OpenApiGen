using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenApiGen;

public static class Json {

    public static JsonSerializerOptions Configure(JsonSerializerOptions options) {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip;
        options.WriteIndented = true;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.AllowOutOfOrderMetadataProperties = true;
        return options;
    }

    public static readonly JsonSerializerOptions Default = Configure(new JsonSerializerOptions());

    public static string Serialize<T>(T obj) {
        return JsonSerializer.Serialize(obj, Default);
    }

    public static T Deserialize<T>(string json) {
        return JsonSerializer.Deserialize<T>(json, Default)!;
    }

}

