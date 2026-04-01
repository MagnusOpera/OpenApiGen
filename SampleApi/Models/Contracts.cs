using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace SampleApi.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProjectState {
    Draft,
    Active,
    Archived
}

public sealed record HelloRequest {
    [Required]
    [MinLength(1)]
    public required string FirstName { get; init; }

    [MinLength(1)]
    public string? LastName { get; init; }
}

public sealed record HelloResponse {
    public required string Message { get; init; }
}

public sealed record UserSummary {
    public required string Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Nickname { get; init; }
    public ProjectState State { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateOnly? NextReviewOn { get; init; }
    public Dictionary<string, string>? Labels { get; init; }
}

public sealed record PatchUser {
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Nickname { get; init; }
    public Dictionary<string, string>? Labels { get; init; }
}

public sealed record Metric {
    public decimal Value { get; init; }
    public string? Unit { get; init; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(CatPet), typeDiscriminator: "cat")]
[JsonDerivedType(typeof(DogPet), typeDiscriminator: "dog")]
public abstract record Animal {
    public required string Name { get; init; }
}

public sealed record CatPet : Animal {
    public int LivesLeft { get; init; }
}

public sealed record DogPet : Animal {
    public bool GoodBoy { get; init; }
}

public sealed record DashboardResponse {
    public required UserSummary Owner { get; init; }
    public ProjectState State { get; init; }
    public required string[] Tags { get; init; }
    public required Dictionary<string, Metric> Metrics { get; init; }
    public required Animal PrimaryPet { get; init; }
    public Animal? BackupPet { get; init; }
}

public sealed record CreateAdoptionRequest {
    [Required]
    public required Animal Pet { get; init; }
    public string? Notes { get; init; }
    public string[]? Labels { get; init; }
}

public sealed record AdoptionRecord {
    public required Guid Id { get; init; }
    public required Animal Pet { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record UploadAssetRequest {
    [Required]
    public required IFormFile File { get; init; }
    public string? Description { get; init; }
}

public sealed record UploadAssetResponse {
    public required string Id { get; init; }
    public required string FileName { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}
