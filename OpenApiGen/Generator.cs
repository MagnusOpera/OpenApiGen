using System.Text;
using System.Text.RegularExpressions;

namespace OpenApiGen;

public static class TypeScriptGenerator {
    public static void Generate(OpenApiDocument document, string outputPath) {
        var tags = new Dictionary<string, List<(string operationId, Operation op, string path, string method)>>();

        foreach (var (path, pathItem) in document.Paths) {
            if (pathItem.Get is not null) Add(tags, pathItem.Get, path, "get");
            if (pathItem.Post is not null) Add(tags, pathItem.Post, path, "post");
            if (pathItem.Put is not null) Add(tags, pathItem.Put, path, "put");
            if (pathItem.Delete is not null) Add(tags, pathItem.Delete, path, "delete");
            if (pathItem.Patch is not null) Add(tags, pathItem.Patch, path, "patch");
        }

        foreach (var (tag, operations) in tags) {
            var sb = new StringBuilder();
            sb.AppendLine($"// === {tag} ===");
            sb.AppendLine("import { AxiosInstance } from \"axios\"");
            sb.AppendLine();
            var indent = 0;
            foreach (var (operationId, op, path, method) in operations) {
                sb.AppendLine($"// === {method} {path} ===");
                var functionName = GenerateFunctionName(path, method);

                string? reqInterface = null;
                if (op.RequestBody?.Content?.TryGetValue("application/json", out var reqContent) == true) {
                    reqInterface = $"{GenerateInterfaceName(path, method)}Request";
                    sb.Append($"export interface {reqInterface} ");
                    sb.AppendLine(GenerateInterfaceBody(indent, reqContent.Schema, reqContent.Schema.Required));
                } else if (op.RequestBody?.Content?.ContainsKey("text/plain") == true) {
                    reqInterface = "string";
                }

                string? resInterface = null;
                foreach (var (responseType, response) in op.Responses) {
                    if (response.Content?.TryGetValue("application/json", out var resContent) == true) {
                        var resTypeInterface = $"{GenerateInterfaceName(path, method)}{responseType}Response";
                        sb.Append($"export interface {resTypeInterface} ");
                        sb.AppendLine(GenerateInterfaceBody(indent, resContent.Schema, resContent.Schema.Required));
                        if (responseType == "200") {
                            resInterface = resTypeInterface;
                        }
                    } else if (response.Content?.ContainsKey("text/plain") == true) {
                        var resTypeInterface = "string";
                        if (responseType == "200") {
                            resInterface = resTypeInterface;
                        }
                    }
                }

                // if (op.Responses?.Content?.TryGetValue("application/json", out var resContent) == true) {
                    //     resInterface = $"{GenerateInterfaceName(path, method)}Response";
                // } else if (op.RequestBody?.Content?.ContainsKey("text/plain") == true) {
                //     reqInterface = "string"
                // }


                // var reqInterface = $"{GenerateInterfaceName(path, method)}Request";
                // var resInterface = $"{GenerateInterfaceName(path, method)}Response";

                // if (method == "get" && op.Parameters is { Count: > 0 }) {
                // } else if (op.RequestBody?.Content?.TryGetValue("application/json", out var reqContent) == true) {
                //     sb.Append($"export interface {reqInterface} ");
                //     sb.AppendLine(GenerateInterfaceBody(indent, reqContent.Schema, reqContent.Schema.Required));
                // }

                // if (op.Responses.TryGetValue("200", out var res) &&
                //     res.Content?.TryGetValue("application/json", out var resContent) == true) {
                //     sb.Append($"export interface {resInterface} ");
                //     sb.AppendLine(GenerateInterfaceBody(indent, resContent.Schema, resContent.Schema.Required));
                // }

                if (method == "get") {
                    var paramsArg = op.Parameters?.Aggregate("", (acc, param) => $"{acc}, {ParameterPrototype(indent, param)}");
                    var queryArgs = op.Parameters?.Select(ParameterQuery);
                    var pathQuery = queryArgs is not null ? $"?{string.Join("&", queryArgs)}" : "";
                    if (!string.IsNullOrEmpty(resInterface)) {
                        sb.AppendLine($"export async function {functionName}(axios: AxiosInstance{paramsArg}): Promise<{resInterface}> {{");
                        sb.AppendLine($"  const response = await axios.get<{resInterface}>(`{ToTemplateString(path) + pathQuery}`)");
                        sb.AppendLine($"  return response.data");
                    } else {
                        sb.AppendLine($"export async function {functionName}(axios: AxiosInstance{paramsArg}): Promise {{");
                        sb.AppendLine($"  await axios.get(`{ToTemplateString(path) + pathQuery}`)");
                    }
                    sb.AppendLine("}");
                } else {
                    sb.AppendLine($"export async function {functionName}(axios: AxiosInstance, request: {reqInterface}): Promise<{resInterface}> {{");
                    sb.AppendLine($"  const response = await axios.{method}<{resInterface}>(\"{path}\", request)");
                    sb.AppendLine($"  return response.data");
                    sb.AppendLine("}");
                }

                sb.AppendLine();
            }

            var outputFilename = Path.Combine(outputPath, $"{tag}.ts");
            File.WriteAllText(outputFilename, sb.ToString());
        }
    }

    private static string ParameterPrototype(int indent, Parameter param) {
        var tsType = MapSchemaType(indent, param.Schema);
        var optional = param.Required != true && param.Schema.Default is null ? "?" : "";
        var def = param.Schema.Default is not null ? $" = {param.Schema.Default}" : "";
        return $"{param.Name}{optional}: {tsType}{def}";
    }

    private static string ParameterQuery(Parameter param) {
        return $"{param.Name}=${{{param.Name}}}";
    }

    private static void Add(Dictionary<string, List<(string, Operation, string, string)>> dict, Operation op, string path, string method) {
        var tag = op.Tags.FirstOrDefault() ?? "Default";
        var id = $"{method}_{Regex.Replace(path.Trim('/'), "[^a-zA-Z0-9]", "_")}";
        if (!dict.ContainsKey(tag)) dict[tag] = new();
        dict[tag].Add((id, op, path, method));
    }

    private static string GenerateInterfaceBody(int indent, Schema schema, List<string>? required) {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        if (schema.Type == "array" && schema.Items is not null) {
            var itemType = MapSchemaType(indent, schema.Items);
            sb.Append(' ', indent);
            sb.AppendLine($"  items: {itemType}[]");
        } else if (schema.Properties is not null) {
            foreach (var (name, prop) in schema.Properties) {
                var isRequired = required?.Contains(name) == true;
                var optional = isRequired ? "" : "?";
                var type = MapSchemaType(indent, prop);
                var def = prop.Default is not null ? $" /* default: {prop.Default} */" : "";
                sb.Append(' ', indent);
                sb.AppendLine($"  {name}{optional}: {type}");
            }
        }
        sb.Append(' ', indent);
        sb.Append('}');
        return sb.ToString();
    }

    private static string MapSchemaType(int indent, Schema s) {
        var t = s.Ref is not null ? "any"
            : s.Type == "string" ? "string"
            : s.Type == "integer" || s.Type == "number" ? "number"
            : s.Type == "boolean" ? "boolean"
            : s.Type == "array" ? $"{MapSchemaType(indent, s.Items ?? new Schema { Type = "any" })}[]"
            : s.Type == "object" && s.Properties is not null ? $"{GenerateInterfaceBody(indent + 2, s, s.Required)}"
            : "any";

        var nullable = s.Nullable == true;
        if (s.Enum is { Count: > 0 })
            return string.Join(" | ", s.Enum.Select(e => $"\"{e}\"")) + (nullable ? " | null" : "");

        return (nullable ? "null | " : "") + t;
    }


    private static string GenerateFunctionName(string path, string method) {
        var parts = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries)
           .Select(p => Regex.Replace(p, "[^a-zA-Z0-9]", ""))
           .Select(Capitalize);
        return method + string.Concat(parts);
    }

    private static string GenerateInterfaceName(string path, string method) {
        var parts = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries)
           .Select(p => Regex.Replace(p, "[^a-zA-Z0-9]", ""))
           .Select(Capitalize);
        return string.Concat(parts) + Capitalize(method);
    }

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];

    public static string ToTemplateString(string path) =>
        Regex.Replace(path, @"\{([^\}]+)\}", @"${$1}");
}
