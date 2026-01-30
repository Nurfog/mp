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

builder.Services.AddOpenApi();

var app = builder.Build();

// Standard Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseAuthorization();

// 1. Root Health Check
app.MapGet("/", () => "Mercado Pago API is running. Documentation: /scalar");

// 2. OpenAPI JSON
app.MapOpenApi();

// 3. Scalar UI (Simplified path)
app.MapScalarApiReference("/scalar");

// 4. API Endpoints
app.MapControllers();

app.Run();
