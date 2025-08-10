var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi().AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
app.MapControllers();
