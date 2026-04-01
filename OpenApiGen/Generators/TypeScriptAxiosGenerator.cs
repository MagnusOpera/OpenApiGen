using System.Text;

namespace OpenApiGen.Generators;

public sealed class TypeScriptAxiosGenerator(Dictionary<string, Schema> components) : TypeScriptHttpClientGenerator(components) {
    protected override string ClientParameterDeclaration => "axios: AxiosInstance";

    protected override void AppendTransportPreamble(StringBuilder sb) {
        sb.AppendLine("import type { AxiosInstance } from \"axios\"");
        sb.AppendLine();
    }

    protected override void AppendTransportRequest(StringBuilder sb, OperationRenderContext context) {
        sb.Append(' ', IndentationSize); sb.AppendLine("const __headers__: Record<string, string> = {};");
        if (context.HasBearer) {
            sb.Append(' ', IndentationSize); sb.AppendLine("__headers__.Authorization = `Bearer ${bearer}`;");
        }

        switch (context.RequestBodyKind) {
            case RequestBodyKind.Json:
                sb.Append(' ', IndentationSize); sb.AppendLine("__headers__[\"Content-Type\"] = \"application/json\";");
                break;
            case RequestBodyKind.Text:
                sb.Append(' ', IndentationSize); sb.AppendLine("__headers__[\"Content-Type\"] = \"text/plain\";");
                break;
        }

        sb.Append(' ', IndentationSize); sb.AppendLine("const __response__ = await axios.request({");
        sb.Append(' ', IndentationSize * 2); sb.AppendLine($"method: \"{context.Method}\",");
        sb.Append(' ', IndentationSize * 2); sb.AppendLine($"url: `{ToTemplateString(context.Path)}${{__queryString__}}`,");

        switch (context.RequestBodyKind) {
            case RequestBodyKind.Json:
                sb.Append(' ', IndentationSize * 2); sb.AppendLine("data: JSON.stringify(request),");
                break;
            case RequestBodyKind.Text:
                sb.Append(' ', IndentationSize * 2); sb.AppendLine("data: request,");
                break;
            case RequestBodyKind.Multipart:
                sb.Append(' ', IndentationSize * 2); sb.AppendLine("data: __form__,");
                break;
        }

        sb.Append(' ', IndentationSize * 2); sb.AppendLine("headers: __headers__,");
        sb.Append(' ', IndentationSize * 2); sb.AppendLine("validateStatus: () => true,");
        if (context.HasBinaryResponse) {
            sb.Append(' ', IndentationSize * 2); sb.AppendLine("responseType: \"blob\",");
        }
        sb.Append(' ', IndentationSize); sb.AppendLine("});");
    }

    protected override string GetResponseValueExpression(ResponseRenderContext response) =>
        response.BodyKind == ResponseBodyKind.None
            ? $"undefined as {response.TypeName}"
            : $"__response__.data as {response.TypeName}";
}
