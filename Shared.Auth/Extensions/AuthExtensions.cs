using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Auth.Abstractions;
using Shared.Auth.Configuration;
using Shared.Auth.Services;

namespace Shared.Auth.Extensions
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection(JwtOptions.SectionName);
            services.Configure<JwtOptions>(jwtSection);

            var jwtOptions = jwtSection.Get<JwtOptions>()
                             ?? throw new InvalidOperationException("Jwt configuration is missing.");

            if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
                throw new InvalidOperationException("Jwt:Issuer is missing.");
            if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
                throw new InvalidOperationException("Jwt:Audience is missing.");
            if (string.IsNullOrWhiteSpace(jwtOptions.Key))
                throw new InvalidOperationException("Jwt:Key is missing.");
            if (jwtOptions.ExpiryMinutes <= 0)
                throw new InvalidOperationException("Jwt:ExpiryMinutes must be greater than zero.");

            services.AddScoped<IJwtTokenService, JwtTokenService>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                        NameClaimType = ClaimTypes.NameIdentifier,
                        RoleClaimType = ClaimTypes.Role
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}
