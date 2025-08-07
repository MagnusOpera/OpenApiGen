namespace OpenApiGen;





public static class Program {
    private static int Main(string[] args) {
        if (args.Length != 3) {
            Console.WriteLine("Usage: OpenApiGen <configuration-file> <openapi-file> <output-dir>");
            return 5;
        }

        var configurationFile = args[0];
        var openapiFile = args[1];
        var outputDir = args[2];

        // var filename = "FundApi-Polymorphic.json";
        // var filename = "FundApi-Multipart.json";
        // var filename = "FundApi.json";

        var apiDefJson = File.ReadAllText(openapiFile);
        var apiDef = Json.Deserialize<OpenApiDocument>(apiDefJson);

        var newContent = Json.Serialize(apiDef);
        File.WriteAllText("FundApi-out.json", newContent);

        if (Directory.Exists(outputDir)) {
            Directory.Delete(outputDir, true);
        }
        Directory.CreateDirectory(outputDir);


        // read configuration
        var configJson = File.ReadAllText(configurationFile);
        var config = Json.Deserialize<Configuration>(configJson);

        var generator = new TypeScriptGenerator(config.SharedSchemas, apiDef.Components?.Schemas ?? []);
        generator.Generate(apiDef, outputDir);
        return 0;
    }
}