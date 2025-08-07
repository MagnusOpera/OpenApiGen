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

        var content = File.ReadAllText(openapiFile);
        var def = Json.Deserialize<OpenApiDocument>(content);

        var newContent = Json.Serialize(def);
        File.WriteAllText("FundApi-out.json", newContent);

        if (Directory.Exists(outputDir)) {
            Directory.Delete(outputDir, true);
        }
        Directory.CreateDirectory(outputDir);


        // read configuration
        var configurationContent = File.ReadAllText(configurationFile);
        var configuration = Json.Deserialize<Configuration>(configurationContent);

        var generator = new TypeScriptGenerator(configuration.Schemas);
        generator.Generate(def, outputDir);
        return 0;
    }
}