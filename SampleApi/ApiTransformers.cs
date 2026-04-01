using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace SampleApi;

[AttributeUsage(AttributeTargets.Method)]
public sealed class DefaultProblemResponseAttribute : Attribute;

[AttributeUsage(AttributeTargets.Method)]
public sealed class TextPlainRequestBodyAttribute : Attribute;

[AttributeUsage(AttributeTargets.Method)]
public sealed class JsonProblemResponsesAttribute : Attribute;

[AttributeUsage(AttributeTargets.Method)]
public sealed class BinaryResponseAttribute(string contentType = "application/octet-stream") : Attribute {
    public string ContentType { get; } = contentType;
}

public sealed class AddBearerSecuritySchemeDocumentTransformer : IOpenApiDocumentTransformer {
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken) {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["bearerAuth"] = new OpenApiSecurityScheme {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Bearer authentication"
        };

        return Task.CompletedTask;
    }
}

public sealed class EnhanceOperationsTransformer : IOpenApiOperationTransformer {
    public async Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken) {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;

        if (!metadata.Any(m => m is AllowAnonymousAttribute) && metadata.OfType<AuthorizeAttribute>().Any()) {
            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement {
                [new OpenApiSecuritySchemeReference("bearerAuth", context.Document, null)] = []
            });
        }

        if (metadata.OfType<DefaultProblemResponseAttribute>().Any()) {
            operation.Responses ??= [];
            operation.Responses["default"] = new OpenApiResponse {
                Description = "Unexpected error",
                Content = new Dictionary<string, OpenApiMediaType> {
                    ["application/json"] = new() {
                        Schema = await context.GetOrCreateSchemaAsync(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), null, cancellationToken)
                    }
                }
            };
        }

        if (metadata.OfType<TextPlainRequestBodyAttribute>().Any() && operation.RequestBody?.Content is { } requestContent) {
            requestContent.Clear();
            requestContent["text/plain"] = new OpenApiMediaType {
                Schema = new OpenApiSchema {
                    Type = JsonSchemaType.String
                }
            };
        }

        var binaryResponse = metadata.OfType<BinaryResponseAttribute>().FirstOrDefault();
        if (binaryResponse is not null
            && operation.Responses is not null
            && operation.Responses.TryGetValue("200", out var binaryOkResponse)
            && binaryOkResponse.Content is { } binaryContent) {
            binaryContent.Clear();
            binaryContent[binaryResponse.ContentType] = new OpenApiMediaType {
                Schema = new OpenApiSchema {
                    Type = JsonSchemaType.String,
                    Format = "binary"
                }
            };
        }

        if (metadata.OfType<JsonProblemResponsesAttribute>().Any() && operation.Responses is not null) {
            foreach (var (statusCode, response) in operation.Responses.Where(pair => !pair.Key.StartsWith("2", StringComparison.Ordinal)).ToArray()) {
                if (response.Content is null) {
                    continue;
                }

                response.Content.Clear();
                response.Content["application/json"] = new OpenApiMediaType {
                    Schema = await context.GetOrCreateSchemaAsync(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), null, cancellationToken)
                };
                operation.Responses[statusCode] = response;
            }
        }

        await Task.CompletedTask;
    }
}
