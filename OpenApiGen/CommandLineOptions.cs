namespace OpenApiGen;

public enum TypeScriptTransport {
    Axios,
    Fetch
}

public readonly record struct CommandLineOptions(
    TypeScriptTransport Transport,
    string OpenApiFile,
    string OutputDir
) {
    public static bool TryParse(string[] args, out CommandLineOptions options) {
        options = default;

        if (args.Length != 4 || args[0] != "--transport") {
            return false;
        }

        if (!Enum.TryParse<TypeScriptTransport>(args[1], ignoreCase: true, out var transport)) {
            return false;
        }

        options = new CommandLineOptions(transport, args[2], args[3]);
        return true;
    }
}
