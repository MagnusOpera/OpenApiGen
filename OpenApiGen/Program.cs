namespace OpenApiGen;





public static class Program {
    private static int Main(string[] args) {
        if (args.Length == 1 && args[0] == "--help") {
            PrintHelp();
            return 0;
        }
        if (!CommandLineOptions.TryParse(args, out var options)) {
            PrintHelp();
            return 5;
        }

        // read api def
        var apiDefJson = File.ReadAllText(options.OpenApiFile);
        var apiDef = Json.Deserialize<OpenApiDocument>(apiDefJson);

        if (Directory.Exists(options.OutputDir)) {
            Directory.Delete(options.OutputDir, true);
        }
        Directory.CreateDirectory(options.OutputDir);

        // generate api
        Generators.TypeScriptHttpClientGenerator generator = options.Transport switch {
            TypeScriptTransport.Axios => new Generators.TypeScriptAxiosGenerator(apiDef.Components?.Schemas ?? []),
            TypeScriptTransport.Fetch => new Generators.TypeScriptFetchGenerator(apiDef.Components?.Schemas ?? []),
            _ => throw new ApplicationException($"Unknown transport {options.Transport}")
        };
        generator.Generate(apiDef, options.OutputDir);
        return 0;
    }

    private static void PrintHelp() {
        Console.WriteLine("OpenApiGen - Generate TypeScript clients from OpenAPI definitions\n");
        Console.WriteLine("Usage: OpenApiGen --transport <axios|fetch> <openapi-file> <output-dir>");
        Console.WriteLine("       OpenApiGen --help\n");
        Console.WriteLine("Arguments:");
        Console.WriteLine("  --transport <value>    Mandatory transport to generate: axios or fetch");
        Console.WriteLine("  <openapi-file>         Path to the OpenAPI definition JSON file");
        Console.WriteLine("  <output-dir>           Output directory for the generated client (will be purged)");
        Console.WriteLine();
    }
}
