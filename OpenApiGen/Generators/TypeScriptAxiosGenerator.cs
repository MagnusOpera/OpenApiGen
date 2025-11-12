using System.Text;
using System.Text.RegularExpressions;

namespace OpenApiGen.Generators;

public class TypeScriptAxiosGenerator(Dictionary<string, Schema> sharedSchemas, Dictionary<string, Schema> components) {

    private const int INDENTATION_SIZE = 4;

    private readonly Dictionary<string, string> _replacedShemas = [];

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

        var bearerRequirement =
            document.Components?.SecuritySchemes?.Where(x => x.Value is HttpSecurityScheme { Scheme: "bearer" })
                    .Select(x => x.Key).FirstOrDefault();

        foreach (var (tag, operations) in tags) {
            var sb = new StringBuilder();
            var outputFilename = Path.Combine(outputPath, $"{tag}.ts");

            // add imports
            if (!File.Exists(outputFilename)) {
                sb.AppendLine($"// === {tag} ===");
                sb.AppendLine("/* eslint-disable @typescript-eslint/no-unused-vars */");
                sb.AppendLine("import type { AxiosInstance } from \"axios\"");
                sb.AppendLine(globalImport);
                sb.AppendLine();
            }

            // generate operations
            foreach (var (operationId, op, path, method) in operations) {
                sb.AppendLine($"// === {method} {path} ===");
                var functionName = GenerateFunctionName(path, method);

                string? reqInterface = null;
                ObjectSchema? multipartSchema = null;
                if (op.RequestBody?.Content?.TryGetValue("application/json", out var reqJsonContent) == true) {
                    reqInterface = $"{GenerateInterfaceName(path, method)}Request";
                    sb.Append($"export type {reqInterface} = ");
                    sb.AppendLine(GenerateType(INDENTATION_SIZE, reqJsonContent.Schema ?? new PrimitiveSchema(), [], []));
                } else if (op.RequestBody?.Content?.TryGetValue("text/plain", out var reqTxtContent) == true) {
                    reqInterface = $"{GenerateInterfaceName(path, method)}Request";
                    sb.Append($"export type {reqInterface} = ");
                    sb.AppendLine(GenerateType(INDENTATION_SIZE, reqTxtContent.Schema ?? new PrimitiveSchema(), [], []));
                } else if (op.RequestBody?.Content?.TryGetValue("multipart/form-data", out var reqMultipartContent) == true) {
                    reqInterface = $"{GenerateInterfaceName(path, method)}Request";
                    sb.Append($"export type {reqInterface} = ");
                    sb.AppendLine(GenerateType(INDENTATION_SIZE, reqMultipartContent.Schema ?? new PrimitiveSchema(), [], []));
                    multipartSchema = reqMultipartContent.Schema as ObjectSchema;
                } else if (op.RequestBody == null) {
                    // void case
                } else {
                    throw new ApplicationException($"Operation {method} {path} has an unhandled request type: {op.RequestBody?.Content?.Keys.FirstOrDefault()}");
                }

                var hasBinaryResponse = false;
                Dictionary<string, string> resDUInterfaces = [];
                foreach (var (responseType, response) in op.Responses) {
                    var responseTypeOrDefault = responseType == "default" ? "0" : responseType;
                    string? resTypeInterface = null;
                    if (response.Content?.TryGetValue("application/json", out var resJsonContent) == true) {
                        resTypeInterface = $"{GenerateInterfaceName(path, method)}{responseTypeOrDefault}Response";
                        resDUInterfaces.Add(responseTypeOrDefault, resTypeInterface);
                        sb.Append($"export type {resTypeInterface} = ");
                        sb.AppendLine(GenerateType(INDENTATION_SIZE, resJsonContent.Schema ?? new PrimitiveSchema(), [], []));
                    } else if (response.Content?.TryGetValue("text/plain", out var resTxtContent) == true) {
                        resTypeInterface = $"{GenerateInterfaceName(path, method)}{responseTypeOrDefault}Response";
                        resDUInterfaces.Add(responseTypeOrDefault, resTypeInterface);
                        sb.Append($"export type {resTypeInterface} = ");
                        sb.AppendLine(GenerateType(INDENTATION_SIZE, resTxtContent.Schema ?? new PrimitiveSchema(), [], []));
                    } else if (response.Content == null) {
                        // void case
                        resTypeInterface = $"{GenerateInterfaceName(path, method)}{responseTypeOrDefault}Response";
                        resDUInterfaces.Add(responseTypeOrDefault, resTypeInterface);
                        sb.Append($"export type {resTypeInterface} = ");
                        sb.AppendLine(GenerateType(INDENTATION_SIZE, new PrimitiveSchema(), [], []));
                    } else {
                        hasBinaryResponse = true;
                        resTypeInterface = $"{GenerateInterfaceName(path, method)}{responseTypeOrDefault}Response";
                        resDUInterfaces.Add(responseTypeOrDefault, resTypeInterface);
                        sb.AppendLine($"export type {resTypeInterface} = Blob");
                    }
                }

                var hasBearer = bearerRequirement is not null && op.Security?.Any(x => x.ContainsKey(bearerRequirement)) == true;
                var bearerArgs = hasBearer ? ", bearer: string" : "";
                var bearerHeader = hasBearer ? ", headers: { Authorization: `Bearer ${bearer}` }" : "";
                var paramArgs = op.Parameters?.Where(x => x.In != "query")?.Aggregate("", (acc, param) => $"{acc}, {ParameterPrototype(param)}");
                var queryArgs = op.Parameters?.Where(x => x.In == "query")?.Aggregate("", (acc, param) => $"{acc}, {ParameterPrototype(param)}");
                var requestType = reqInterface is not null ? $", request: {reqInterface}" : null;
                var requestArg = reqInterface is not null ? $", request" : null;

                var queryParams = op.Parameters?.Where(x => x.In == "query").ToList();

                // generate function with discriminated unions
                var resInterface = string.Join(" | ", resDUInterfaces);
                sb.AppendLine($"export async function {functionName}(axios: AxiosInstance{bearerArgs}{paramArgs}{requestType}{queryArgs}): Promise<{resInterface}> {{");

                if (queryParams?.Any() == true) {
                    sb.Append(' ', INDENTATION_SIZE); sb.AppendLine("const __query__: string[] = [];");
                    foreach (var param in queryParams) {
                        sb.Append(' ', INDENTATION_SIZE); sb.AppendLine(ParameterQueryInitializer(param));
                    }
                    sb.Append(' ', INDENTATION_SIZE); sb.AppendLine("const __queryString__ = __query__.length ? `?${__query__.join(\"&\")}` : \"\";");
                } else {
                    sb.Append(' ', INDENTATION_SIZE); sb.AppendLine("const __queryString__ = \"\";");
                }

                if (multipartSchema is not null) {
                    sb.Append(' ', INDENTATION_SIZE); sb.AppendLine("const __form__ = new FormData()");
                    foreach (var (propName, propSchema) in multipartSchema.Properties ?? []) {
                        var accessor = $"request.{ToFieldName(propName)}";
                        sb.Append(' ', INDENTATION_SIZE); sb.AppendLine($"if ({accessor} !== undefined) __form__.append(\"{propName}\", {accessor})");
                    }
                    sb.Append(' ', INDENTATION_SIZE); sb.AppendLine($"const __response__ = (await axios.{method}(`{ToTemplateString(path)}${{__queryString__}}`, __form__))");
                } else {
                    var responseTypeConfig = hasBinaryResponse ? ", responseType: 'blob' as const" : "";
                    sb.Append(' ', INDENTATION_SIZE); 
                    sb.AppendLine($"const __response__ = await axios.{method}(`{ToTemplateString(path)}${{__queryString__}}`{requestArg}, {{ validateStatus: () => true{bearerHeader}{responseTypeConfig} }})");
                }
                sb.Append(' ', INDENTATION_SIZE); sb.AppendLine("switch (__response__.status) {");
                Response? defaultResponse = null;
                foreach (var (responseType, response) in op.Responses) {
                    if (responseType == "default") {
                        defaultResponse = response;
                    } else {
                        var resTypeInterface = $"{GenerateInterfaceName(path, method)}{responseType}Response";
                        sb.Append(' ', INDENTATION_SIZE * 2); sb.AppendLine($"case {responseType}: return [{responseType}, __response__.data as {resTypeInterface}]");
                    }
                }
                if (defaultResponse is not null) {
                    var resTypeInterface = $"{GenerateInterfaceName(path, method)}0Response";
                    sb.Append(' ', INDENTATION_SIZE * 2); sb.AppendLine($"default: return [0, __response__.data as {resTypeInterface}]");
                } else {
                    sb.Append(' ', INDENTATION_SIZE * 2); sb.AppendLine("default: throw Error(`Unexpected status ${__response__.status}`)");
                }
                sb.Append(' ', INDENTATION_SIZE); sb.AppendLine("}");

                sb.AppendLine("}");
                sb.AppendLine();
            }

            File.AppendAllText(outputFilename, sb.ToString());
        }

        foreach (var (source, target) in _replacedShemas) {
            Console.WriteLine($"WARNING: component {source} replaced with {target}");
        }
    }

    private string ParameterPrototype(Parameter param) {
        var tsType = GenerateType(0, param.Schema, [], []);
        var optional = param.Required != true ? "?" : "";
        return $"{param.Name}{optional}: {tsType}";
    }

    private static string ParameterQueryInitializer(Parameter param) {
        return $"if ({param.Name} !== undefined) __query__.push(`{param.Name}=${{encodeURIComponent({param.Name})}}`);";
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
            var composed = compSchema.AnyOf ?? compSchema.OneOf ?? [];
            var variants = string.Join(" | ", composed.Select(variant => {
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
                var fieldname = ToFieldName(name);
                sb.AppendLine($"{fieldname}{optional}: {type}");
            }
            if (objSchema.AdditionalProperties is not null) {
                var mapValueType = GenerateType(indent + INDENTATION_SIZE, objSchema.AdditionalProperties, [], []);
                sb.Append(' ', indent);
                sb.AppendLine($"[key: string]: {mapValueType}");
            }
            sb.Append(' ', indent - INDENTATION_SIZE);
            sb.Append('}');
            return sb.ToString();
        } else if (schema is EnumSchema enumSchema) {
            return string.Join(" | ", enumSchema.Enum.Where(e => e is not null).Select(e => $"\"{e}\""));
        } else if (schema is PrimitiveSchema primSchema) {
            var types = primSchema.Type ?? ["void"];
            return string.Join(" | ", types.Select(type => (type, primSchema.Format) switch {
                ("string", "binary") => "Blob",
                ("string", _) => "string",
                ("integer", _) => "number",
                ("number", _) => "number",
                ("boolean", _) => "boolean",
                ("null", _) => "null",
                ("void", "date") => "string",
                ("void", "date-time") => "string",
                _ => "void"
            }));
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

    private static string GenerateRefName(string name) =>
        Regex.Replace(name, "[^a-zA-Z]", "");

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..].ToLowerInvariant();

    private static string Lowerize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s[1..];

    private static string ToTemplateString(string path) =>
        Regex.Replace(path, @"\{([^\}]+)\}", @"${$1}");

    private static string ToFieldName(string name) {
        name = Regex.Replace(name, "[^a-zA-Z0-9]", "");
        return Lowerize(name);
    }
}
