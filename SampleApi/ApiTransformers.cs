// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.OpenApi;
// using Microsoft.OpenApi.Models;

// namespace SampleApi;

// public sealed class AddBearerSecuritySchemeDocumentTransformer : IOpenApiDocumentTransformer {
//     public Task TransformAsync(
//         OpenApiDocument document,
//         OpenApiDocumentTransformerContext context,
//         CancellationToken cancellationToken) {
//         document.Components ??= new OpenApiComponents();

//         // Define a single HTTP bearer scheme for the whole doc
//         document.Components.SecuritySchemes["bearerAuth"] = new OpenApiSecurityScheme {
//             Type = SecuritySchemeType.Http,
//             Scheme = "bearer",
//             BearerFormat = "JWT",
//             Description = "JWT Bearer authentication"
//         };

//         return Task.CompletedTask;
//     }
// }

// public sealed class AddBearerRequirementForAuthorizeOperationTransformer : IOpenApiOperationTransformer {
//     public Task TransformAsync(
//         OpenApiOperation operation,
//         OpenApiOperationTransformerContext context,
//         CancellationToken cancellationToken) {
//         var metadata = context.Description.ActionDescriptor.EndpointMetadata;

//         // Skip if [AllowAnonymous]
//         if (metadata.Any(m => m is AllowAnonymousAttribute)) {
//             return Task.CompletedTask;
//         }

//         // Only care about AuthorizeAttribute (including any subclasses)
//         var hasAuthorize = metadata.OfType<AuthorizeAttribute>().Any();
//         if (!hasAuthorize) {
//             return Task.CompletedTask;
//         }

//         // Add operation.security = [{ bearerAuth: [] }]
//         operation.Security ??= [];
//         var bearerRef = new OpenApiSecurityScheme {
//             Reference = new OpenApiReference {
//                 Type = ReferenceType.SecurityScheme,
//                 Id = "bearerAuth"
//             }
//         };
//         operation.Security.Add(new OpenApiSecurityRequirement {
//             [bearerRef] = []
//         });
//         return Task.CompletedTask;
//     }
// }
