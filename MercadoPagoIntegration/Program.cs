using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
builder.Services.AddScoped<MercadoPagoIntegration.Services.IMercadoPagoService, MercadoPagoIntegration.Services.MercadoPagoService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi("v1");

var app = builder.Build();

// 1. Configure OpenAPI & Scalar (Early in the pipeline)
app.MapOpenApi();
app.MapScalarApiReference(options => 
{
    options.WithTitle("Mercado Pago API Reference")
           .WithTheme(ScalarTheme.Moon)
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

// 2. Health check / Root
app.MapGet("/", () => Results.Ok("Mercado Pago API is running. Documentation: /scalar-api-reference"));

// 3. Static Files & Redirection
app.UseStaticFiles(); // Only serve files if they exist
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// 4. API Endpoints
app.UseAuthorization();
app.MapControllers();

app.Run();
