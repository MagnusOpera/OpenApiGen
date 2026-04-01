namespace OpenApiGen.Tests;

public sealed class CommandLineOptionsTests {
    [Fact]
    public void TryParse_requires_transport_switch() {
        var success = CommandLineOptions.TryParse(["Examples/PetStore.json", "generated"], out _);

        Assert.False(success);
    }

    [Theory]
    [InlineData("axios", TypeScriptTransport.Axios)]
    [InlineData("fetch", TypeScriptTransport.Fetch)]
    public void TryParse_accepts_supported_transports(string transport, TypeScriptTransport expectedTransport) {
        var success = CommandLineOptions.TryParse(["--transport", transport, "Examples/PetStore.json", "generated"], out var options);

        Assert.True(success);
        Assert.Equal(expectedTransport, options.Transport);
        Assert.Equal("Examples/PetStore.json", options.OpenApiFile);
        Assert.Equal("generated", options.OutputDir);
    }

    [Fact]
    public void TryParse_rejects_unknown_transport() {
        var success = CommandLineOptions.TryParse(["--transport", "xmlhttp", "Examples/PetStore.json", "generated"], out _);

        Assert.False(success);
    }
}
