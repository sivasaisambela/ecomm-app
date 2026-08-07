using OrderService.Application.Mappers;
using OrderService.Application.Services;
using OrderService.Domain.Interfaces;
using OrderService.Infrastructure.Clients;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using OrderService.Application.Validators;
using OrderService.Api.Middleware;

namespace OrderService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =========================================================================
            // 1. DATABASE CONFIGURATION
            // =========================================================================
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");

            builder.Services.AddDbContext<OrderContext>(options =>
                options.UseSqlServer(connectionString));

            // =========================================================================
            // 2. DEPENDENCY INJECTION REGISTRATION
            // =========================================================================
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderApplicationService, OrderApplicationService>();

            // =========================================================================
            // 3. AUTOMAPPER REGISTRATION
            // =========================================================================
            // =========================================================================
            // 3. AUTOMAPPER REGISTRATION
            // =========================================================================
            builder.Services.AddAutoMapper((Action<AutoMapper.IMapperConfigurationExpression>)null!, typeof(OrderMappingProfile).Assembly);
            // =========================================================================
            // 4. HTTP CLIENT CONFIGURATION (Microservice Communication)
            // =========================================================================
            var productServiceUrl = builder.Configuration["ExternalServices:ProductServiceUrl"]
                ?? throw new InvalidOperationException("ExternalServices:ProductServiceUrl configuration is missing.");

            builder.Services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
            {
                client.BaseAddress = new Uri(productServiceUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            // =========================================================================
            // 5. STANDARD WEB API SERVICES
            // =========================================================================
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderDtoValidator>();

            var app = builder.Build();

            // =========================================================================
            // 6. HTTP REQUEST PIPELINE (Middleware Configuration)
            // =========================================================================
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API v1");
                });
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}