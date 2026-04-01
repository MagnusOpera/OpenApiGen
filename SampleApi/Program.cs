using System.Text.Json.Serialization;
using SampleApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi(options => {
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
app.Run();
