
namespace OpenApiGen;


public record Configuration {
    public Dictionary<string, Schema>? SharedSchemas { get; init; }
}

