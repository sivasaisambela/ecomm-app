using ProductService.Api.Extensions;
using ProductService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ProductService.Api.Middleware;
using ProductService.Application.Services;
using ProductService.Domain.Interfaces;
using ProductService.Infrastructure.Repositories;
using ProductService.Application.Mappers;
using FluentValidation;
using ProductService.Application.Validators;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. ADD SERVICES & CONTROLLERS
// ============================================
builder.Services.AddControllers();

// ============================================
// 2. DATABASE CONFIGURATION
// ============================================
builder.Services.AddDbContext<ProductContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================
// 3. DEPENDENCY INJECTION REGISTRATION
// ============================================
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductApplicationService, ProductApplicationService>();

// ============================================
// 4. AUTOMAPPER REGISTRATION
// ============================================
builder.Services.AddAutoMapper(typeof(ProductMapper));

// ============================================
// 5. CORS CONFIGURATION
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ============================================
// 6. FLUENT VALIDATION REGISTRATION
// ============================================
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ============================================
// 7. MIDDLEWARE PIPELINE
// ============================================
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Global Exception Handler Middleware (custom, maps domain exceptions -> proper status codes)
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Service API v1");
    });
}

app.UseAuthorization();
app.MapControllers();

app.Run();