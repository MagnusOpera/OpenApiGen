using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenApiGen;


public sealed class SchemaConverter : JsonConverter<Schema> {
    public override Schema? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        using var doc = JsonDocument.ParseValue(ref reader);
        var el = doc.RootElement;
        var declaredTypes = GetDeclaredTypes(el);
        var nonNullTypes = declaredTypes?.Where(type => type != "null").ToList() ?? [];
        var nullableFromTypeArray = declaredTypes is not null && declaredTypes.Contains("null") && nonNullTypes.Count > 0;

        if (el.TryGetProperty("$ref", out _)) {
            return ApplyNullable(DeserializeAs<RefSchema>(el, options), nullableFromTypeArray);
        }

        if (el.TryGetProperty("anyOf", out _) || el.TryGetProperty("oneOf", out _)) {
            return ApplyNullable(DeserializeAs<ComposedSchema>(el, options), nullableFromTypeArray);
        }

        if (el.TryGetProperty("items", out _)) {
            return ApplyNullable(DeserializeAs<ArraySchema>(el, options), nullableFromTypeArray);
        }

        if (el.TryGetProperty("enum", out _)) {
            return ApplyNullable(DeserializeAs<EnumSchema>(el, options), nullableFromTypeArray);
        }

        if ((declaredTypes?.Contains("object") == true)
            || el.TryGetProperty("properties", out _)
            || el.TryGetProperty("required", out _)
            || el.TryGetProperty("additionalProperties", out _)) {
            return ApplyNullable(DeserializeAs<ObjectSchema>(el, options), nullableFromTypeArray);
        }

        // Special handling for OpenAPI 3.1: type can be string or array
        if (declaredTypes is not null) {
            // Manually build PrimitiveSchema
            var prim = new PrimitiveSchema {
                Type = nullableFromTypeArray ? nonNullTypes : declaredTypes,
                Format = el.TryGetProperty("format", out var fmt) ? fmt.GetString() : null,
                Pattern = el.TryGetProperty("pattern", out var pat) ? pat.GetString() : null,
                Default = el.TryGetProperty("default", out var def) ? def.ToString() : null,
                Nullable = nullableFromTypeArray
            };
            return prim;
        }

        // fallback
        return ApplyNullable(DeserializeAs<PrimitiveSchema>(el, options), nullableFromTypeArray)
               ?? throw new JsonException("Could not deserialize OpenAPI Schema.");
    }

    public override void Write(Utf8JsonWriter writer, Schema value, JsonSerializerOptions options) {
        if (value is RefSchema refSchema) {
            SerializeAs(writer, refSchema, options);
        } else if (value is ComposedSchema compSchema) {
            SerializeAs(writer, compSchema, options);
        } else if (value is ArraySchema arrSchema) {
            SerializeAs(writer, arrSchema, options);
        } else if (value is EnumSchema enumSchema) {
            SerializeAs(writer, enumSchema, options);
        } else if (value is ObjectSchema objectSchema) {
            SerializeAs(writer, objectSchema, options);
        } else if (value is PrimitiveSchema primSchema) {
            SerializeAs(writer, primSchema, options);
        } else {
            throw new JsonException($"Could not serialize OpenAPI Schema.");
        }
    }

    private static T? DeserializeAs<T>(JsonElement el, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<T>(el.GetRawText(), options);

    private static void SerializeAs<T>(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, options);

    private static Schema? ApplyNullable(Schema? schema, bool nullableFromTypeArray) =>
        schema is null || !nullableFromTypeArray ? schema : schema with { Nullable = true };

    private static List<string>? GetDeclaredTypes(JsonElement el) {
        if (!el.TryGetProperty("type", out var typeProp)) {
            return null;
        }

        var types = new List<string>();
        if (typeProp.ValueKind == JsonValueKind.String && typeProp.GetString() is string scalarType) {
            types.Add(scalarType);
            return types;
        }

        if (typeProp.ValueKind != JsonValueKind.Array) {
            return null;
        }

        foreach (var item in typeProp.EnumerateArray()) {
            if (item.ValueKind == JsonValueKind.String && item.GetString() is string arrayType) {
                types.Add(arrayType);
            }
        }

        return types;
    }
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
