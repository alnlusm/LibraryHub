var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "LibraryHub API Gateway",
    routes = new[] { "/identity/api/auth", "/catalog/api/books", "/ordering/api/orders" }
}));

app.MapReverseProxy();
app.Run();
