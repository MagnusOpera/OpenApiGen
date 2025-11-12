using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi(); //options => {
//     options.AddDocumentTransformer(new AddBearerSecuritySchemeDocumentTransformer());
//     options.AddOperationTransformer(new AddBearerRequirementForAuthorizeOperationTransformer());
// });
builder.Services.AddControllers();

builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict);

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
app.MapControllers();
