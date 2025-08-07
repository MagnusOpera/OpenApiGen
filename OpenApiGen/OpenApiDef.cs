
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


    public virtual bool Equals(Schema? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Type == other.Type &&
               Format == other.Format &&
               Nullable == other.Nullable &&
               Ref == other.Ref &&
               Enumerable.SequenceEqual(Required ?? [], other.Required ?? []) &&
               Enumerable.SequenceEqual(Enum ?? [], other.Enum ?? []) &&
               DeepEquals(Properties, other.Properties) &&
               DeepEquals(AnyOf, other.AnyOf) &&
               Equals(Items, other.Items) &&
               Equals(Discriminator, other.Discriminator) &&
               Equals(Default, other.Default);
    }

    public override int GetHashCode()
    {
        // Be cautious: hash code logic should match equality logic
        var hash = new HashCode();
        hash.Add(Type);
        hash.Add(Format);
        hash.Add(Nullable);
        hash.Add(Ref);
        AddEnumerableHash(hash, Required);
        AddEnumerableHash(hash, Enum);
        AddDictionaryHash(hash, Properties);
        AddEnumerableHash(hash, AnyOf);
        hash.Add(Items);
        hash.Add(Discriminator);
        hash.Add(Default);
        return hash.ToHashCode();
    }

    private static bool DeepEquals<K, V>(Dictionary<K, V>? a, Dictionary<K, V>? b)
    {
        if (a is null || b is null) return a == b;
        if (a.Count != b.Count) return false;
        return a.All(pair => b.TryGetValue(pair.Key, out var bv) && Equals(pair.Value, bv));
    }

    private static bool DeepEquals<T>(List<T>? a, List<T>? b)
    {
        return Enumerable.SequenceEqual(a ?? [], b ?? []);
    }

    private static void AddEnumerableHash<T>(HashCode hash, IEnumerable<T>? values)
    {
        if (values is null) return;
        foreach (var v in values)
            hash.Add(v);
    }

    private static void AddDictionaryHash<K, V>(HashCode hash, Dictionary<K, V>? dict)
    {
        if (dict is null) return;
        foreach (var kvp in dict.OrderBy(p => p.Key?.ToString()))
        {
            hash.Add(kvp.Key);
            hash.Add(kvp.Value);
        }
    }
}

public record Discriminator {
    public required string PropertyName { get; init; }
    public Dictionary<string, string>? Mapping { get; init; }
}
