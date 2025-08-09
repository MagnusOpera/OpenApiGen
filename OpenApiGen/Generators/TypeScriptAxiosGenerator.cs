using System.Text;
using System.Text.RegularExpressions;

namespace OpenApiGen.Generators;

public class TypeScriptAxiosGenerator(Dictionary<string, Schema> sharedSchemas, Dictionary<string, Schema> components) {

    private const int INDENTATION_SIZE = 4;

    private string GenerateGlobalTypes(string outputPath) {
        var sb = new StringBuilder();
        foreach (var (name, schema) in sharedSchemas) {
            if (schema.Nullable is not null) throw new ApplicationException($"Shared type {name} shall not be nullable.");
            sb.Append($"export type {name} = ");
            sb.AppendLine(RawGenerateType(INDENTATION_SIZE, schema, [], []));
        }
        var outputFilename = Path.Combine(outputPath, "__shared_schemas__.ts");
        File.WriteAllText(outputFilename, sb.ToString());

        var types = string.Join(", ", sharedSchemas.Select(x => x.Key));
        return $"import type {{ {types} }} from \"./__shared_schemas__\"";
    }


    public void Generate(OpenApiDocument document, string outputPath) {
        var globalImport = GenerateGlobalTypes(outputPath);

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
            sb.AppendLine("/* eslint-disable @typescript-eslint/no-unused-vars */");
            sb.AppendLine("import type { AxiosInstance } from \"axios\"");
            sb.AppendLine(globalImport);
            sb.AppendLine();
            foreach (var (operationId, op, path, method) in operations) {
                sb.AppendLine($"// === {method} {path} ===");
                var functionName = GenerateFunctionName(path, method);

                string? reqInterface = null;
                if (op.RequestBody?.Content?.TryGetValue("application/json", out var reqJsonContent) == true) {
                    reqInterface = $"{GenerateInterfaceName(path, method)}Request";
                    sb.Append($"export type {reqInterface} = ");
                    sb.AppendLine(GenerateType(INDENTATION_SIZE, reqJsonContent.Schema, [], []));
                } else if (op.RequestBody?.Content?.TryGetValue("multipart/form-data", out var reqMultipartContent) == true) {
                    reqInterface = $"{GenerateInterfaceName(path, method)}Request";
                    sb.Append($"export type {reqInterface} = ");
                    sb.AppendLine(GenerateType(INDENTATION_SIZE, reqMultipartContent.Schema, [], []));
                } else if (op.RequestBody?.Content?.ContainsKey("text/plain") == true) {
                    reqInterface = "string";
                }

                string? resInterface = null;
                foreach (var (responseType, response) in op.Responses) {
                    string? resTypeInterface = null;
                    if (response.Content?.TryGetValue("application/json", out var resContent) == true) {
                        var errResponseType = responseType == "200" ? "" : responseType;
                        resTypeInterface = $"{GenerateInterfaceName(path, method)}{errResponseType}Response";
                        sb.Append($"export type {resTypeInterface} = ");
                        sb.AppendLine(GenerateType(INDENTATION_SIZE, resContent.Schema, [], []));
                    } else if (response.Content?.ContainsKey("text/plain") == true) {
                        resTypeInterface = "string";
                    }
                    if (responseType == "200") {
                        resInterface = resTypeInterface;
                    }
                }

                var paramArgs = op.Parameters?.Aggregate("", (acc, param) => $"{acc}, {ParameterPrototype(param)}");
                var queryArgs = op.Parameters?.Where(x => x.In == "query").Select(ParameterQuery);
                var query = queryArgs?.Any() == true ? $"?{string.Join("&", queryArgs)}" : "";
                var requestType = reqInterface is not null ? $", request: {reqInterface}" : null;
                var requestArg = reqInterface is not null ? $", request" : null;
                if (!string.IsNullOrEmpty(resInterface)) {
                    sb.AppendLine($"export async function {functionName}(axios: AxiosInstance{paramArgs}{requestType}): Promise<{resInterface}> {{");
                    sb.Append(' ', INDENTATION_SIZE);
                    sb.AppendLine($"return (await axios.{method}<{resInterface}>(`{ToTemplateString(path) + query}`{requestArg})).data");
                } else {
                    sb.AppendLine($"export async function {functionName}(axios: AxiosInstance{paramArgs}{requestType}): Promise {{");
                    sb.Append(' ', INDENTATION_SIZE);
                    sb.AppendLine($"await axios.{method}(`{ToTemplateString(path) + query}`{requestArg})");
                }
                sb.AppendLine("}");
                sb.AppendLine();
            }

            var outputFilename = Path.Combine(outputPath, $"{tag}.ts");
            File.WriteAllText(outputFilename, sb.ToString());
        }
    }

    private string ParameterPrototype(Parameter param) {
        var tsType = GenerateType(0, param.Schema, [], []);
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

    private string RawGenerateType(int indent, Schema schema, string[] composedRequired, Dictionary<string, Schema> composedProperties) {
        if (schema is RefSchema refSchema) {
            var componentName = refSchema.Ref.Split("/").Last();
            var compSchema = components[componentName];
            return GenerateType(indent, compSchema, composedRequired, composedProperties);
        } else if (schema is ComposedSchema compSchema) {
            string[] variantComposedRequired = [.. composedRequired, .. compSchema.Required ?? []];
            Dictionary<string, Schema> variantComposedProperties = new([.. composedProperties, .. compSchema.Properties ?? []]);
            var variants = string.Join(" | ", compSchema.AnyOf.Select(variant => {
                return GenerateType(indent, variant, variantComposedRequired, variantComposedProperties);
            }));
            return variants;
        } else if (schema is ArraySchema arrSchema) {
            return $"Array<{GenerateType(indent, arrSchema.Items, [], [])}>";
        } else if (schema is ObjectSchema objSchema) {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            string[] required = [.. composedRequired, .. objSchema.Required ?? []];
            Dictionary<string, Schema> properties = new([.. composedProperties, .. objSchema.Properties ?? []]);
            foreach (var (name, prop) in properties) {
                var optional = required.Contains(name) ? "" : "?";
                var type = GenerateType(indent + INDENTATION_SIZE, prop, [], []);
                sb.Append(' ', indent);
                sb.AppendLine($"{name}{optional}: {type}");
            }
            sb.Append(' ', indent - INDENTATION_SIZE);
            sb.Append('}');
            return sb.ToString();
        } else if (schema is EnumSchema enumSchema) {
            return string.Join(" | ", enumSchema.Enum.Select(e => $"\"{e}\""));
        } else if (schema is PrimitiveSchema primSchema) {
            return (primSchema.Type, primSchema.Format) switch {
                ("string", "binary") => "File",
                ("string", _) => "string",
                ("integer", _) => "number",
                ("number", _) => "number",
                ("boolean", _) => "boolean",
                _ => "any"
            };
        } else {
            throw new ApplicationException("Unknown schema type.");
        }
    }

    private string GenerateType(int indent, Schema schema, string[] composedRequired, Dictionary<string, Schema> composedProperties) {
        var jsonSchema = Json.Serialize(schema with { Nullable = null });
        var type = sharedSchemas.Where(x => Json.Serialize(x.Value with { Nullable = null }) == jsonSchema).Select(x => x.Key).FirstOrDefault();
        type ??= RawGenerateType(indent, schema, composedRequired, composedProperties);

        var nullable = schema.Nullable == true;
        return nullable ? $"null | {type}" : type;
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

    private static string ToTemplateString(string path) =>
        Regex.Replace(path, @"\{([^\}]+)\}", @"${$1}");
}
