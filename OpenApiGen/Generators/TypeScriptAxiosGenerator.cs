using System.Data.Common;
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
                } else if (op.RequestBody == null) {
                    // void case
                } else {
                    throw new ApplicationException($"Operation {method} {path} has an unhandled request type: {op.RequestBody?.Content?.Keys.FirstOrDefault()}");
                }

                Dictionary<string, string> resDUInterfaces = [];
                foreach (var (responseType, response) in op.Responses) {
                    string? resTypeInterface = null;
                    if (response.Content?.TryGetValue("application/json", out var resJsonContent) == true) {
                        resTypeInterface = $"{GenerateInterfaceName(path, method)}{responseType}Response";
                        resDUInterfaces.Add(responseType, resTypeInterface);
                        sb.Append($"export type {resTypeInterface} = ");
                        sb.AppendLine(GenerateType(INDENTATION_SIZE, resJsonContent.Schema ?? new PrimitiveSchema(), [], []));
                    } else if (response.Content?.TryGetValue("text/plain", out var resTxtContent) == true) {
                        resTypeInterface = $"{GenerateInterfaceName(path, method)}{responseType}Response";
                        resDUInterfaces.Add(responseType, resTypeInterface);
                        sb.Append($"export type {resTypeInterface} = ");
                        sb.AppendLine(GenerateType(INDENTATION_SIZE, resTxtContent.Schema ?? new PrimitiveSchema(), [], []));
                    } else if (response.Content == null) {
                        // void case
                    } else {
                        throw new ApplicationException($"Operation {method} {path} has an unhandled response type: {response.Content?.Keys.FirstOrDefault()}");
                    }
                }
                if (!resDUInterfaces.ContainsKey("200")) {
                    throw new ApplicationException($"Operation {method} {path} has no success response 200");
                }

                var hasBearer = bearerRequirement is not null && op.Security?.Any(x => x.ContainsKey(bearerRequirement)) == true;
                var bearerArgs = hasBearer ? ", bearer: string" : "";
                var bearerHeader = hasBearer ? ", headers: { Authorization: `Bearer ${bearer}` }" : "";
                var paramArgs = op.Parameters?.Aggregate("", (acc, param) => $"{acc}, {ParameterPrototype(param)}");
                var queryArgs = op.Parameters?.Where(x => x.In == "query").Select(ParameterQuery);
                var query = queryArgs?.Any() == true ? $"{string.Concat(queryArgs)}" : "";
                var requestType = reqInterface is not null ? $", request: {reqInterface}" : null;
                var requestArg = reqInterface is not null ? $", request" : null;

                // generate function with exception
                if (!resDUInterfaces.ContainsValue("void")) {
                    sb.AppendLine($"export async function {functionName}(axios: AxiosInstance{paramArgs}{requestType}): Promise<{resDUInterfaces["200"]}> {{");
                    sb.Append(' ', INDENTATION_SIZE);
                    sb.AppendLine($"return (await axios.{method}<{resDUInterfaces["200"]}>(`{ToTemplateString(path) + query}`{requestArg})).data");
                } else {
                    sb.AppendLine($"export async function {functionName}(axios: AxiosInstance{paramArgs}{requestType}): Promise<void> {{");
                    sb.Append(' ', INDENTATION_SIZE * 2); sb.AppendLine($"await axios.{method}(`{ToTemplateString(path) + query}`{requestArg})");
                }
                sb.AppendLine("}");

                // generate function with discriminated unions
                var resInterface = string.Join(" | ", resDUInterfaces);
                sb.AppendLine($"export async function {functionName}Async(axios: AxiosInstance{bearerArgs}{paramArgs}{requestType}): Promise<{resInterface}> {{");
                sb.Append(' ', INDENTATION_SIZE); sb.AppendLine($"const resp = await axios.{method}(`{ToTemplateString(path) + query}`{requestArg}, {{ validateStatus: () => true{bearerHeader} }})");
                sb.Append(' ', INDENTATION_SIZE); sb.AppendLine("switch (resp.status) {");
                foreach (var (responseType, response) in op.Responses) {
                    var resTypeInterface = $"{GenerateInterfaceName(path, method)}{responseType}Response";
                    sb.Append(' ', INDENTATION_SIZE * 2); sb.AppendLine($"case {responseType}: return [{responseType}, resp.data as {resTypeInterface}]");
                }
                sb.Append(' ', INDENTATION_SIZE * 2); sb.AppendLine("default: throw `Unexpected status ${resp.status}`");
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

    private static string ParameterQuery(Parameter param, int index) {
        var sep = index == 0 ? "?" : "&";
        if (param.Schema.Default is null) {
            return $"${{{param.Name} === undefined ? `` : `{sep}{param.Name}=${{encodeURIComponent({param.Name})}}`}}";
        } else {
            return $"${{{param.Name} === undefined ? `{sep}{param.Name}=${{encodeURIComponent({param.Schema.Default})}}` : `{sep}{param.Name}=${{encodeURIComponent({param.Name})}}`}}";
        }
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
            // NOTE: Workaround for a bug in Microsoft.Extensions.ApiDescription.Server 9.0.8,
            //       where nullable types are incorrectly emitted in the OpenAPI definition
            //       as having type "any". This heuristic locates an equivalent schema
            //       without nullability (but with the same required members) and then
            //       applies the intended nullability to it. It assumes the base, non-nullable
            //       schema is emitted correctly, which appears to be the case.
            var componentName = refSchema.Ref.Split("/").Last();
            var compSchema = components[componentName];
            // already replaced ?
            if (_replacedShemas.TryGetValue(componentName, out var repCompNamec)) {
                compSchema = components[repCompNamec] with { Nullable = compSchema.Nullable };

                // target component has any member ?
            } else if (compSchema is ObjectSchema compObjSchema
                && compObjSchema.Properties?.Any(x => (x.Value as PrimitiveSchema)?.Type is null) == true) {

                // try to find another component with same equivalent name and same members but with types this time
                // note this can lamely fail of course
                var compMembers = compObjSchema.Properties?.Keys.ToList() ?? [];
                var candidateName = GenerateRefName(componentName);
                var candidateComp = components.FirstOrDefault(candidate => {
                    if (candidate.Key.StartsWith(candidateName) && candidate.Value is ObjectSchema candidateObjShema
                        && candidateObjShema.Properties?.All(x => x.Value is PrimitiveSchema { Type: not null } || x.Value is RefSchema) == true) {
                        var candidateMembers = candidateObjShema.Properties?.Keys.ToList() ?? [];
                        return candidateMembers.SequenceEqual(compMembers);
                    }
                    return false;
                });

                if (candidateComp.Value is not null && componentName != candidateComp.Key) {
                    _replacedShemas[componentName] = candidateComp.Key;
                    compSchema = candidateComp.Value with { Nullable = compSchema.Nullable };
                }
            }

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
                _ => "void"
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

    private static string GenerateRefName(string name) =>
        Regex.Replace(name, "[^a-zA-Z]", "");

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..].ToLowerInvariant();

    private static string ToTemplateString(string path) =>
        Regex.Replace(path, @"\{([^\}]+)\}", @"${$1}");
}
