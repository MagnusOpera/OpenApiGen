namespace OpenApiGen.Tests;

public sealed class TypeScriptGeneratorSnapshotTests {
    [Theory]
    [InlineData(TypeScriptTransport.Axios, "axios")]
    [InlineData(TypeScriptTransport.Fetch, "fetch")]
    public void SampleApi_generation_matches_snapshot(TypeScriptTransport transport, string snapshotName) {
        using var output = GenerationTestSupport.Generate(transport, "Examples/SampleApi.json");

        GenerationTestSupport.AssertMatchesSnapshot(Path.Combine("Snapshots", "SampleApi", snapshotName), output.DirectoryPath);
    }
}
