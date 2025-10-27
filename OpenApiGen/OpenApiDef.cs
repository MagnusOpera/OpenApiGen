
using System.Text.Json.Serialization;

namespace OpenApiGen;


public record OpenApiDocument {
    public required Dictionary<string, PathItem> Paths { get; init; }
    public Components? Components { get; init; }
}

public record Components {
    public Dictionary<string, Schema>? Schemas { get; init; }
    public Dictionary<string, SecurityScheme>? SecuritySchemes { get; init; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(HttpSecurityScheme), typeDiscriminator: "http")]
[JsonDerivedType(typeof(ApiKeySecurityScheme), typeDiscriminator: "apiKey")]
public record SecurityScheme {
    public required string Description { get; init; }
}

public record HttpSecurityScheme : SecurityScheme {
    public required string Scheme { get; init; }
    public required string BearerFormat { get; init; }
}

public record ApiKeySecurityScheme : SecurityScheme {
    public required string In { get; init; }
    public required string Name { get; init; }
}

public record PathItem {
    public Operation? Get { get; init; }
    public Operation? Post { get; init; }
    public Operation? Put { get; init; }
    public Operation? Delete { get; init; }
    public Operation? Patch { get; init; }
}

public record Operation {
    public required List<string> Tags { get; init; }
    public RequestBody? RequestBody { get; init; }
    public required Dictionary<string, Response> Responses { get; init; }
    public List<Parameter>? Parameters { get; init; }
    public Dictionary<string, string[]>[]? Security { get; init; }
}

public record RequestBody {
    public required Dictionary<string, MediaType> Content { get; init; }
    public bool? Required { get; init; }
}

public record MediaType {
    public Schema? Schema { get; init; }
}

public record Response {
    public required string Description { get; init; }
    public Dictionary<string, MediaType>? Content { get; init; }
}

public record Parameter {
    public required string Name { get; init; }
    public required string In { get; init; }
    public bool? Required { get; init; }
    public required Schema Schema { get; init; }
}

[JsonConverter(typeof(SchemaConverter))]
public abstract record Schema {
    public bool? Nullable { get; init; } // deprecated in 3.1
    public object? Default { get; init; }
}

public sealed record EnumSchema : Schema {
    public required List<string> Enum { get; init; }
}

public sealed record RefSchema : Schema {
    [JsonPropertyName("$ref")] public required string Ref { get; init; }
}

public sealed record ArraySchema : Schema {
    public required Schema Items { get; init; }
}

public record ObjectSchema : Schema {
    public Dictionary<string, Schema>? Properties { get; init; }
    public List<string>? Required { get; init; }

    public Schema? AdditionalProperties { get; init; }
}

public sealed record ComposedSchema : ObjectSchema {
    public required List<Schema> AnyOf { get; init; }
    public Discriminator? Discriminator { get; init; }
}

public sealed record Discriminator {
    public required string PropertyName { get; init; }
}

public sealed record PrimitiveSchema : Schema {
    public string? Type { get; init; }   // "string", "integer", "number", "boolean"
    public string? Format { get; init; } // "date-time", "uuid", etc.
    public string[]? Types { get; init; } // new in 3.1
}
