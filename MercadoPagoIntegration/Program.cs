using Scalar.AspNetCore;
using MercadoPagoIntegration.Services;
using MercadoPago.Config;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Configurar el SDK con tu Test Access Token
MercadoPagoConfig.AccessToken = builder.Configuration["MercadoPago:AccessToken"];

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<IMercadoPagoService, MercadoPagoService>();
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

builder.Services.AddOpenApi();

var app = builder.Build();

// Standard Middleware
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles(); 
app.UseCors("AllowAll");
app.UseAuthorization();

// 1. Root Health Check
app.MapGet("/health", () => "Mercado Pago API is running.");

// 2. OpenAPI JSON
app.MapOpenApi();

// 3. Scalar UI (Simplified path)
app.MapScalarApiReference("/scalar");

// 4. API Endpoints
app.MapControllers();

app.Run();
