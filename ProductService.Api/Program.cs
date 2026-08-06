using ProductService.Api.Extensions;

using Microsoft.EntityFrameworkCore;
using ProductService.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// ADD SERVICES
// ============================================

// Add controllers
builder.Services.AddControllers();



// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Authorization
builder.Services.AddAuthorization();

// ============================================
// BUILD APP
// ============================================

var app = builder.Build();

// ============================================
// MIDDLEWARE
// ============================================

// Use CORS
app.UseCors("AllowAll");

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Service API v1");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}
app.UseExceptionHandling();
// HTTPS redirection
app.UseHttpsRedirection();

// Authorization
app.UseAuthorization();

// Map controllers
app.MapControllers();

// ============================================
// DATABASE MIGRATION
// ============================================

// Apply migrations on startup


// ============================================
// RUN APP
// ============================================
// Exception handling

app.Run();