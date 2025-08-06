using System.Text;
using System.Text.RegularExpressions;

namespace OpenApiGen;

public static class TypeScriptGenerator {
    public static void Generate(OpenApiDocument document, string outputPath) {
        var tags = new Dictionary<string, List<(string operationId, Operation op, string path, string method)>>();

        foreach (var (path, pathItem) in document.Paths) {
            if (pathItem.Get is not null) {
                Add(tags, pathItem.Get, path, "get");
            }

            if (pathItem.Post is not null) {
                Add(tags, pathItem.Post, path, "post");
            }

            if (pathItem.Put is not null) {
                Add(tags, pathItem.Put, path, "put");
            }

            if (pathItem.Delete is not null) {
                Add(tags, pathItem.Delete, path, "delete");
            }

            if (pathItem.Patch is not null) {
                Add(tags, pathItem.Patch, path, "patch");
            }
        }

        foreach (var (tag, operations) in tags) {
            var sb = new StringBuilder();
            sb.AppendLine($"// === {tag} ===");
            sb.AppendLine("import { AxiosInstance } from \"axios\"");
            sb.AppendLine();
            foreach (var (operationId, op, path, method) in operations) {
                sb.AppendLine($"// === {method} {path} ===");
                var functionName = GenerateFunctionName(path, method);

                string? reqInterface = null;
                if (op.RequestBody?.Content?.TryGetValue("application/json", out var reqJsonContent) == true) {
                    reqInterface = $"{GenerateInterfaceName(path, method)}Request";
                    sb.Append($"export interface {reqInterface} ");
                    sb.AppendLine(GenerateInterfaceBody(0, reqJsonContent.Schema, reqJsonContent.Schema.Required));
                } else if (op.RequestBody?.Content?.TryGetValue("multipart/form-data", out var reqMultipartContent) == true) {
                    reqInterface = $"{GenerateInterfaceName(path, method)}Request";
                    sb.Append($"export interface {reqInterface} ");
                    sb.AppendLine(GenerateInterfaceBody(0, reqMultipartContent.Schema, reqMultipartContent.Schema.Required));
                } else if (op.RequestBody?.Content?.ContainsKey("text/plain") == true) {
                    reqInterface = "string";
                }

                string? resInterface = null;
                foreach (var (responseType, response) in op.Responses) {
                    string? resTypeInterface = null;
                    if (response.Content?.TryGetValue("application/json", out var resContent) == true) {
                        var errResponseType = responseType == "200" ? "" : responseType;
                        resTypeInterface = $"{GenerateInterfaceName(path, method)}{errResponseType}Response";
                        sb.Append($"export interface {resTypeInterface} ");
                        sb.AppendLine(GenerateInterfaceBody(0, resContent.Schema, resContent.Schema.Required));
                    } else if (response.Content?.ContainsKey("text/plain") == true) {
                        resTypeInterface = "string";
                    }
                    if (responseType == "200") {
                        resInterface = resTypeInterface;
                    }
                }

                var paramsArg = op.Parameters?.Aggregate("", (acc, param) => $"{acc}, {ParameterPrototype(param)}");
                var queryArgs = op.Parameters?.Select(ParameterQuery);
                var pathQuery = queryArgs is not null ? $"?{string.Join("&", queryArgs)}" : "";
                var request = reqInterface is not null ? $", request: {reqInterface}" : null;
                var requestArg = reqInterface is not null ? $", request" : null;
                if (!string.IsNullOrEmpty(resInterface)) {
                    sb.AppendLine($"export async function {functionName}(axios: AxiosInstance{paramsArg}{request}): Promise<{resInterface}> {{");
                    sb.AppendLine($"  const response = await axios.{method}<{resInterface}>(`{ToTemplateString(path) + pathQuery}`{requestArg})");
                    sb.AppendLine($"  return response.data");
                } else {
                    sb.AppendLine($"export async function {functionName}(axios: AxiosInstance{paramsArg}{request}): Promise {{");
                    sb.AppendLine($"  await axios.{method}(`{ToTemplateString(path) + pathQuery}`{requestArg})");
                }
                sb.AppendLine("}");
                sb.AppendLine();
            }

            var outputFilename = Path.Combine(outputPath, $"{tag}.ts");
            File.WriteAllText(outputFilename, sb.ToString());
        }
    }

    private static string ParameterPrototype(Parameter param) {
        var tsType = MapSchemaType(0, param.Schema);
        var optional = param.Required != true ? "?" : "";
        return $"{param.Name}{optional}: {tsType}";
    }

    private static string ParameterQuery(Parameter param) {
        var def = param.Schema.Default is not null ? $" ?? {param.Schema.Default}" : "";
        return $"{param.Name}=${{{param.Name}{def}}}";
    }

    private static void Add(Dictionary<string, List<(string, Operation, string, string)>> dict, Operation op, string path, string method) {
        var tag = op.Tags.FirstOrDefault() ?? "Default";
        var id = $"{method}_{Regex.Replace(path.Trim('/'), "[^a-zA-Z0-9]", "_")}";
        if (!dict.TryGetValue(tag, out var value)) {
            value = [];
            dict[tag] = value;
        }

        value.Add((id, op, path, method));
    }

    private static string GenerateInterfaceBody(int indent, Schema schema, List<string>? required) {
        var sb = new StringBuilder();
        sb.AppendLine("{");

        if (schema.AnyOf is not null) {
            sb.Append(' ', indent + 2);
            var variants = schema.AnyOf.Select((variant, i) => MapSchemaType(indent + 2, variant));
            sb.AppendLine($"  value: {string.Join(" | ", variants)};");
        } else if (schema.Type == "array") {
            var itemType = MapSchemaType(indent, schema.Items!);
            sb.Append(' ', indent);
            sb.AppendLine($"  items: {itemType}[]");
        } else if (schema.Properties is not null) {
            foreach (var (name, prop) in schema.Properties) {
                var isRequired = required?.Contains(name) == true;
                var optional = isRequired ? "" : "?";
                var type = MapSchemaType(indent, prop);
                sb.Append(' ', indent);
                sb.AppendLine($"  {name}{optional}: {type}");
            }
        }
        sb.Append(' ', indent);
        sb.Append('}');
        return sb.ToString();
    }

    private static string MapSchemaType(int indent, Schema s) {
        var t =
            s.AnyOf is not null ? string.Join(" | ", s.AnyOf.Select(subSchema => GenerateInterfaceBody(indent + 2, subSchema, s.Required)))
            : s.Ref is not null ? "any"
            : s.Enum is not null ? string.Join(" | ", s.Enum.Select(e => $"\"{e}\""))
            : s.Type == "string" ? (s.Format is null ? "string" : "File")
            : s.Type == "integer" || s.Type == "number" ? "number"
            : s.Type == "boolean" ? "boolean"
            : s.Type == "array" ? $"{MapSchemaType(indent, s.Items ?? new Schema { Type = "any" })}[]"
            : s.Type == "object" ? $"{GenerateInterfaceBody(indent + 2, s, s.Required)}"
            : "any";

        var nullable = s.Nullable == true;
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
