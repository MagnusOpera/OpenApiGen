using System.Text;
using System.Text.RegularExpressions;

namespace OpenApiGen;

public static class TypeScriptGenerator {
    public static string Generate(OpenApiDocument document) {
        var tags = new Dictionary<string, List<(string operationId, Operation op, string path, string method)>>();

        foreach (var (path, pathItem) in document.Paths) {
            if (pathItem.Get is not null) Add(tags, pathItem.Get, path, "get");
            if (pathItem.Post is not null) Add(tags, pathItem.Post, path, "post");
            if (pathItem.Put is not null) Add(tags, pathItem.Put, path, "put");
            if (pathItem.Delete is not null) Add(tags, pathItem.Delete, path, "delete");
            if (pathItem.Patch is not null) Add(tags, pathItem.Patch, path, "patch");
        }

        var sb = new StringBuilder();
        foreach (var (tag, operations) in tags) {
            sb.AppendLine($"// === {tag} ===");
            foreach (var (operationId, op, path, method) in operations) {
                var functionName = GenerateFunctionName(path, method);
                var reqInterface = $"{functionName}Request";
                var resInterface = $"{functionName}Response";

                if (method == "get" && op.Parameters is { Count: > 0 }) {
                    sb.AppendLine($"export interface {reqInterface} {{");
                    foreach (var param in op.Parameters) {
                        var tsType = MapSchemaType(param.Schema);
                        var optional = param.Required != true ? "?" : "";
                        var def = param.Schema.Default is not null ? $" /* default: {param.Schema.Default} */" : "";
                        sb.AppendLine($"  {param.Name}{optional}: {tsType};{def}");
                    }
                    sb.AppendLine("}");
                } else if (op.RequestBody?.Content?.TryGetValue("application/json", out var reqContent) == true) {
                    sb.AppendLine($"export interface {reqInterface} {{");
                    GenerateInterfaceBody(sb, reqContent.Schema, reqContent.Schema.Required);
                    sb.AppendLine("}");
                }

                if (op.Responses.TryGetValue("200", out var res) &&
                    res.Content?.TryGetValue("application/json", out var resContent) == true) {
                    sb.AppendLine($"export interface {resInterface} {{");
                    GenerateInterfaceBody(sb, resContent.Schema, resContent.Schema.Required);
                    sb.AppendLine("}");
                }

                if (method == "get") {
                    var paramsArg = op.Parameters is { Count: > 0 } ? $"params: {reqInterface}" : "";
                    sb.AppendLine($"export function {functionName}(axios: AxiosInstance, {paramsArg}): Promise<{resInterface}> {{");
                    sb.AppendLine($"  return axios.get(\"{path}\"{(paramsArg != "" ? ", { params }" : "")}).then(r => r.data)");
                    sb.AppendLine("}");
                } else {
                    sb.AppendLine($"export function {functionName}(axios: AxiosInstance, request: {reqInterface}): Promise<{resInterface}> {{");
                    sb.AppendLine($"  return axios.{method}(\"{path}\", request).then(r => r.data)");
                    sb.AppendLine("}");
                }

                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static void Add(Dictionary<string, List<(string, Operation, string, string)>> dict, Operation op, string path, string method) {
        var tag = op.Tags.FirstOrDefault() ?? "Default";
        var id = $"{method}_{Regex.Replace(path.Trim('/'), "[^a-zA-Z0-9]", "_")}";
        if (!dict.ContainsKey(tag)) dict[tag] = new();
        dict[tag].Add((id, op, path, method));
    }

    private static void GenerateInterfaceBody(StringBuilder sb, Schema schema, List<string>? required) {
        if (schema.Type == "array" && schema.Items is not null) {
            var itemType = MapSchemaType(schema.Items);
            sb.AppendLine($"  items: {itemType}[];");
            return;
        }

        if (schema.Properties is null) return;

        foreach (var (name, prop) in schema.Properties) {
            var isRequired = required?.Contains(name) == true;
            var optional = isRequired ? "" : "?";
            var type = MapSchemaType(prop);
            var def = prop.Default is not null ? $" /* default: {prop.Default} */" : "";
            sb.AppendLine($"  {name}{optional}: {type};{def}");
        }
    }

    private static string MapSchemaType(Schema s) {
        var t = s.Ref is not null ? "any"
            : s.Type == "string" ? "string"
            : s.Type == "integer" || s.Type == "number" ? "number"
            : s.Type == "boolean" ? "boolean"
            : s.Type == "array" ? $"{MapSchemaType(s.Items ?? new Schema { Type = "any" })}[]"
            : s.Type == "object" && s.Properties is not null ? "{ [key: string]: any }"
            : "any";

        var nullable = s.Nullable == true;
        if (s.Enum is { Count: > 0 })
            return string.Join(" | ", s.Enum.Select(e => $"\"{e}\"")) + (nullable ? " | null" : "");

        return t + (nullable ? " | null" : "");
    }


    private static string GenerateFunctionName(string path, string method) {
        var parts = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries)
           .Select(p => Regex.Replace(p, "[^a-zA-Z0-9]", ""))
           .Select(Capitalize);
        return string.Concat(parts) + Capitalize(method);
    }

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];
}
