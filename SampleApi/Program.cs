using SampleApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi(options => {
    options.AddDocumentTransformer(new AddBearerSecuritySchemeDocumentTransformer());
    options.AddOperationTransformer(new AddBearerRequirementForAuthorizeOperationTransformer());
});
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
app.MapControllers();
