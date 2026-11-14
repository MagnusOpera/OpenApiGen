using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OpenApi;
using SampleApi;
using SampleApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi(options => {
    options.CreateSchemaReferenceId = typeInfo => {
        var id = OpenApiOptions.CreateDefaultSchemaReferenceId(typeInfo);
        return id is not null && typeInfo.Type.Namespace == typeof(UserSummary).Namespace
            ? $"{typeInfo.Type.Namespace}.{id}"
            : id;
    };
    options.AddDocumentTransformer<AddBearerSecuritySchemeDocumentTransformer>();
    options.AddOperationTransformer<EnhanceOperationsTransformer>();
});
builder.Services.AddAuthorization();
builder.Services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
#if NET11_0_OR_GREATER
app.MapMethods("/search", ["QUERY"], (SearchRequest request) => request.Term)
    .WithTags("Search");
#endif
app.Run();
