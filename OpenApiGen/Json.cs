using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenApiGen;


public sealed class SchemaConverter : JsonConverter<Schema> {
    public override Schema? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        using var doc = JsonDocument.ParseValue(ref reader);
        var el = doc.RootElement;

        if (el.TryGetProperty("$ref", out _))
            return DeserializeAs<RefSchema>(el, options);

        if (el.TryGetProperty("anyOf", out _))
            return DeserializeAs<ComposedSchema>(el, options);

        if (el.TryGetProperty("items", out _))
            return DeserializeAs<ArraySchema>(el, options);

        if (el.TryGetProperty("enum", out _))
            return DeserializeAs<EnumSchema>(el, options);

        if ((el.TryGetProperty("type", out var t) && t.ValueKind == JsonValueKind.String && t.GetString() == "object")
            || el.TryGetProperty("properties", out _)
            || el.TryGetProperty("required", out _))
            return DeserializeAs<ObjectSchema>(el, options);

        // fallback
        return DeserializeAs<PrimitiveSchema>(el, options)
               ?? throw new JsonException("Could not deserialize OpenAPI Schema.");
    }

    public override void Write(Utf8JsonWriter writer, Schema value, JsonSerializerOptions options) {
        if (value is RefSchema refSchema)
            SerializeAs(writer, refSchema, options);

        else if (value is ComposedSchema compSchema)
            SerializeAs(writer, compSchema, options);

        else if (value is ArraySchema arrSchema)
            SerializeAs(writer, arrSchema, options);

        else if (value is EnumSchema enumSchema)
            SerializeAs(writer, enumSchema, options);

        else if (value is ObjectSchema objectSchema)
            SerializeAs(writer, objectSchema, options);

        else if (value is PrimitiveSchema primSchema)
            SerializeAs(writer, primSchema, options);

        else
            throw new JsonException($"Could not serialize OpenAPI Schema.");        
    }

    private static T? DeserializeAs<T>(JsonElement el, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<T>(el.GetRawText(), options);

    private static void SerializeAs<T>(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, options);
}



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

