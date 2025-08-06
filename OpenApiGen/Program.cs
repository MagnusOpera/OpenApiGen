namespace OpenApiGen;





public static class Program {
    private static void Main(string[] args) {
        var content = File.ReadAllText("FundApiSmall.json");
        var def = Json.Deserialize<OpenApiDocument>(content);

        var newContent = Json.Serialize(def);
        File.WriteAllText("FundApiSmall-out.json", newContent);


        var api = TypeScriptGenerator.Generate(def);
        File.WriteAllText("FundApi.ts", api);
    }
}