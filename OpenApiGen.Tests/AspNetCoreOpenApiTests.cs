using System.Text.Json;

namespace OpenApiGen.Tests;

public sealed class AspNetCoreOpenApiTests {
    private static string DocumentPath => Path.Combine(AppContext.BaseDirectory, "openapi", "SampleApi.json");

    [Fact]
    public void Untagged_query_operation_is_exported_from_the_default_module() {
        var document = Json.Deserialize<OpenApiDocument>("""
            {
              "openapi": "3.2.0",
              "paths": {
                "/lookup": {
                  "query": {
                    "responses": { "204": { "description": "No content" } }
                  }
                }
              }
            }
            """);
        using var output = GenerationTestSupport.Generate(TypeScriptTransport.Fetch, document);
        Assert.Contains("export async function queryLookup(", File.ReadAllText(Path.Combine(output.DirectoryPath, "Default.ts")));
        Assert.Contains("export * from './Default'", File.ReadAllText(Path.Combine(output.DirectoryPath, "index.ts")));
    }

    [Fact]
    public void Sample_emits_the_framework_default_document_with_qualified_components_and_binary_responses() {
        using var document = JsonDocument.Parse(File.ReadAllText(DocumentPath));
        var root = document.RootElement;
        var schemas = root.GetProperty("components").GetProperty("schemas");
        Assert.True(schemas.TryGetProperty("SampleApi.Models.UserSummary", out _));

        var paths = root.GetProperty("paths");
        var downloadSchema = paths.GetProperty("/Files/{id}/content").GetProperty("get")
            .GetProperty("responses").GetProperty("200").GetProperty("content")
            .GetProperty("application/octet-stream").GetProperty("schema");
#if NET11_0_OR_GREATER
        Assert.StartsWith("3.2.", root.GetProperty("openapi").GetString());
        Assert.Equal("#/components/schemas/FileContentResult", downloadSchema.GetProperty("$ref").GetString());
        downloadSchema = schemas.GetProperty("FileContentResult");
        Assert.True(paths.GetProperty("/search").TryGetProperty("query", out _));
#else
        Assert.StartsWith("3.1.", root.GetProperty("openapi").GetString());
#endif
        Assert.Equal("string", downloadSchema.GetProperty("type").GetString());
        Assert.Equal("binary", downloadSchema.GetProperty("format").GetString());
    }

    [Theory]
    [InlineData(TypeScriptTransport.Axios, "axios")]
    [InlineData(TypeScriptTransport.Fetch, "fetch")]
    public void Generated_aspnet_document_preserves_the_client_contract(TypeScriptTransport transport, string snapshotName) {
        var document = Json.Deserialize<OpenApiDocument>(File.ReadAllText(DocumentPath));
        using var output = GenerationTestSupport.Generate(transport, document);
#if NET11_0_OR_GREATER
        var search = GenerationTestSupport.ReadGeneratedFile(output.DirectoryPath, "Search.ts");
        Assert.Contains("export type SearchQueryRequest = {\n    term: string\n}", search);
        Assert.Contains("export type SearchQuery200Response = string", search);
        Assert.Contains("method: \"QUERY\",", search);
        Assert.Contains(transport == TypeScriptTransport.Fetch
            ? "body: JSON.stringify(request),"
            : "data: JSON.stringify(request),", search);

        var indexPath = Path.Combine(output.DirectoryPath, "index.ts");
        var index = File.ReadAllText(indexPath);
        var export = $"export * from './Search'{Environment.NewLine}";
        Assert.Contains(export, index);
        File.WriteAllText(indexPath, index.Replace(export, "", StringComparison.Ordinal));
        File.Delete(Path.Combine(output.DirectoryPath, "Search.ts"));
#endif
        GenerationTestSupport.AssertMatchesSnapshot(Path.Combine("Snapshots", "SampleApi", snapshotName), output.DirectoryPath);
    }
}
