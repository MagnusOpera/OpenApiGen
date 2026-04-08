using System.Text;

namespace OpenApiGen.Generators;

public sealed class TypeScriptFetchGenerator(Dictionary<string, Schema> components) : TypeScriptHttpClientGenerator(components) {
    protected override string ClientParameterDeclaration => "fetcher: typeof fetch";

    protected override void AppendTransportPreamble(StringBuilder sb, bool requiresJsonResponseReader) {
        if (requiresJsonResponseReader) {
            sb.AppendLine("import { __readJson__ } from \"./_fetch\"");
            sb.AppendLine();
        }
    }

    protected override void EmitTransportSupportFiles(string outputPath, bool requiresJsonResponseReader) {
        if (!requiresJsonResponseReader) {
            return;
        }

        var sb = new StringBuilder();
        AppendFileHeader(sb);
        sb.AppendLine("export async function __readJson__(response: Response): Promise<unknown> {");
        sb.Append(' ', IndentationSize); sb.AppendLine("const __text__ = await response.text();");
        sb.Append(' ', IndentationSize); sb.AppendLine("return __text__ ? JSON.parse(__text__) : undefined;");
        sb.AppendLine("}");
        File.WriteAllText(Path.Combine(outputPath, "_fetch.ts"), sb.ToString());
    }

    protected override void AppendTransportRequest(StringBuilder sb, OperationRenderContext context) {
        sb.Append(' ', IndentationSize); sb.AppendLine("const __headers__: Record<string, string> = {};");
        if (context.HasBearer) {
            sb.Append(' ', IndentationSize); sb.AppendLine("__headers__.Authorization = `Bearer ${bearer}`;");
        }

        string? bodyExpression = null;
        switch (context.RequestBodyKind) {
            case RequestBodyKind.Json:
                sb.Append(' ', IndentationSize); sb.AppendLine("__headers__[\"Content-Type\"] = \"application/json\";");
                bodyExpression = "JSON.stringify(request)";
                break;
            case RequestBodyKind.Text:
                sb.Append(' ', IndentationSize); sb.AppendLine("__headers__[\"Content-Type\"] = \"text/plain\";");
                bodyExpression = "request";
                break;
            case RequestBodyKind.Multipart:
                bodyExpression = "__form__";
                break;
        }

        sb.Append(' ', IndentationSize); sb.AppendLine("const __requestInit__: RequestInit = {");
        sb.Append(' ', IndentationSize * 2); sb.AppendLine($"method: \"{context.Method.ToUpperInvariant()}\",");
        sb.Append(' ', IndentationSize * 2); sb.AppendLine("headers: __headers__,");
        if (bodyExpression is not null) {
            sb.Append(' ', IndentationSize * 2); sb.AppendLine($"body: {bodyExpression},");
        }
        sb.Append(' ', IndentationSize); sb.AppendLine("};");
        sb.Append(' ', IndentationSize); sb.AppendLine($"const __response__ = await fetcher(`{ToTemplateString(context.Path)}${{__queryString__}}`, __requestInit__);");
    }

    protected override string GetResponseValueExpression(ResponseRenderContext response) {
        if (response.IsVoidLikeType) {
            return response.BodyKind switch {
                ResponseBodyKind.None => $"undefined as {response.TypeName}",
                ResponseBodyKind.Json => $"(await __readJson__(__response__), undefined as {response.TypeName})",
                ResponseBodyKind.Text => $"(await __response__.text(), undefined as {response.TypeName})",
                ResponseBodyKind.Binary => $"(await __response__.blob(), undefined as {response.TypeName})",
                _ => throw new ApplicationException($"Unhandled response body kind {response.BodyKind}")
            };
        }

        return response.BodyKind switch {
            ResponseBodyKind.None => $"undefined as {response.TypeName}",
            ResponseBodyKind.Json => $"(await __readJson__(__response__)) as {response.TypeName}",
            ResponseBodyKind.Text => $"(await __response__.text()) as {response.TypeName}",
            ResponseBodyKind.Binary => $"(await __response__.blob()) as {response.TypeName}",
            _ => throw new ApplicationException($"Unhandled response body kind {response.BodyKind}")
        };
    }
}
