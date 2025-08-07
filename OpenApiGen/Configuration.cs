
namespace OpenApiGen;


public record Configuration {
    public required Dictionary<string, Schema> SharedSchemas { get; init; } = [];
}

