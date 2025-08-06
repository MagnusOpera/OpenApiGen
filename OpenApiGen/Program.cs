namespace OpenApiGen;





public static class Program {
    private static void Main(string[] args) {
        var content = File.ReadAllText("FundApi.json");
        var def = Json.Deserialize<OpenApiDocument>(content);

        var newContent = Json.Serialize(def);
        File.WriteAllText("FundApi-out.json", newContent);


        if (Directory.Exists("generated")) {
            Directory.Delete("generated", true);
        }
        Directory.CreateDirectory("generated");
        TypeScriptGenerator.Generate(def, "generated");
    }
}