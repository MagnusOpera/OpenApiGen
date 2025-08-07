
namespace OpenApiGen;


public record Configuration {
    public required Dictionary<string, Schema> Schemas { get; init; } = [];
}

