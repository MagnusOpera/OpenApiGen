
namespace OpenApiGen;


public record OpenApiDocument {
    public required Dictionary<string, PathItem> Paths { get; init; }
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
}

public record RequestBody {
    public required Dictionary<string, MediaType> Content { get; init; }
    public bool? Required { get; init; }
}

public record MediaType {
    public required Schema Schema { get; init; }
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

public record Schema {
    public string? Type { get; init; }
    public string? Format { get; init; }
    public List<string>? Required { get; init; }
    public Dictionary<string, Schema>? Properties { get; init; }
    public Schema? Items { get; init; }
    public List<Schema>? AnyOf { get; init; }
    public string? Ref { get; init; }
    public List<string>? Enum { get; init; }
    public Discriminator? Discriminator { get; init; }
    public bool? Nullable { get; init; }
    public object? Default { get; init; }
}

public record Discriminator {
    public required string PropertyName { get; init; }
    public Dictionary<string, string>? Mapping { get; init; }
}
