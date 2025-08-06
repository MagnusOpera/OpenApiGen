namespace OpenApiGen;





public static class Program {
    private static void Main(string[] args) {
        // var filename = "FundApi-Polymorphic.json";
        var filename = "FundApi.json";

        var content = File.ReadAllText(filename);
        var def = Json.Deserialize<OpenApiDocument>(content);

        var newContent = Json.Serialize(def);
        File.WriteAllText("FundApi-out.json", newContent);

        var outputDir = Path.GetFileNameWithoutExtension(filename);
        if (Directory.Exists(outputDir)) {
            Directory.Delete(outputDir, true);
        }
        Directory.CreateDirectory(outputDir);
        TypeScriptGenerator.Generate(def, outputDir);
    }
}