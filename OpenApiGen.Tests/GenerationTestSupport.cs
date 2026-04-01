using OpenApiGen.Generators;

namespace OpenApiGen.Tests;

internal sealed class TemporaryDirectory : IDisposable {
    public TemporaryDirectory() {
        DirectoryPath = Path.Combine(Path.GetTempPath(), $"openapigen-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(DirectoryPath);
    }

    public string DirectoryPath { get; }

    public void Dispose() {
        if (Directory.Exists(DirectoryPath)) {
            Directory.Delete(DirectoryPath, recursive: true);
        }
    }
}

internal static class GenerationTestSupport {
    public static string RepoRoot => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));

    public static TemporaryDirectory Generate(TypeScriptTransport transport, string relativeOpenApiPath) {
        var openApiPath = Path.Combine(RepoRoot, relativeOpenApiPath);
        var document = Json.Deserialize<OpenApiDocument>(File.ReadAllText(openApiPath));
        var output = new TemporaryDirectory();
        CreateGenerator(transport, document.Components?.Schemas ?? []).Generate(document, output.DirectoryPath);
        return output;
    }

    public static void AssertMatchesSnapshot(string snapshotRelativePath, string actualDirectory) {
        var expectedDirectory = Path.Combine(RepoRoot, "OpenApiGen.Tests", snapshotRelativePath);
        var expectedFiles = Directory.GetFiles(expectedDirectory, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(expectedDirectory, path))
            .OrderBy(path => path)
            .ToArray();
        var actualFiles = Directory.GetFiles(actualDirectory, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(actualDirectory, path))
            .OrderBy(path => path)
            .ToArray();

        Assert.Equal(expectedFiles, actualFiles);

        foreach (var relativePath in expectedFiles) {
            var expected = NormalizeLineEndings(File.ReadAllText(Path.Combine(expectedDirectory, relativePath)));
            var actual = NormalizeLineEndings(File.ReadAllText(Path.Combine(actualDirectory, relativePath)));
            Assert.Equal(expected, actual);
        }
    }

    public static string ReadGeneratedFile(string directoryPath, string fileName) =>
        NormalizeLineEndings(File.ReadAllText(Path.Combine(directoryPath, fileName)));

    private static TypeScriptHttpClientGenerator CreateGenerator(TypeScriptTransport transport, Dictionary<string, Schema> components) =>
        transport switch {
            TypeScriptTransport.Axios => new TypeScriptAxiosGenerator(components),
            TypeScriptTransport.Fetch => new TypeScriptFetchGenerator(components),
            _ => throw new ApplicationException($"Unknown transport {transport}")
        };

    private static string NormalizeLineEndings(string content) =>
        content.ReplaceLineEndings("\n").TrimEnd() + "\n";
}
