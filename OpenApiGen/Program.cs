namespace OpenApiGen;





public static class Program {
    private static int Main(string[] args) {
        if (args.Length == 1 && args[0] == "--help") {
            PrintHelp();
            return 0;
        }
        if (args.Length != 2) {
            PrintHelp();
            return 5;
        }

        var openapiFile = args[0];
        var outputDir = args[1];

        // read api def
        var apiDefJson = File.ReadAllText(openapiFile);
        var apiDef = Json.Deserialize<OpenApiDocument>(apiDefJson);

        if (Directory.Exists(outputDir)) {
            Directory.Delete(outputDir, true);
        }
        Directory.CreateDirectory(outputDir);

        // generate api
        var generator = new Generators.TypeScriptAxiosGenerator(apiDef.Components?.Schemas ?? []);
        generator.Generate(apiDef, outputDir);
        return 0;
    }

    private static void PrintHelp() {
        Console.WriteLine("OpenApiGen - Generate TypeScript clients from OpenAPI definitions\n");
        Console.WriteLine("Usage: OpenApiGen <openapi-file> <output-dir>");
        Console.WriteLine("       OpenApiGen --help\n");
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <openapi-file>         Path to the OpenAPI definition JSON file");
        Console.WriteLine("  <output-dir>           Output directory for the generated client (will be purged)");
        Console.WriteLine();
    }
}
