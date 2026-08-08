using AuthService.Application.DTOs.Requests;
using AuthService.Application.DTOs.Responses;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Shared.Auth.Abstractions;
using Shared.Auth.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AuthService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly JwtOptions _jwtOptions;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IOptions<JwtOptions> jwtOptions,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _jwtOptions = jwtOptions.Value;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

            if (user is null || !user.IsActive)
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            var token = _jwtTokenService.GenerateToken(user.Id.ToString(), user.Email, user.Role);

            return new LoginResponse
            {
                AccessToken = token,
                ExpiresIn = _jwtOptions.ExpiryMinutes * 60,
                UserId = user.Id.ToString(),
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required.");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Password is required.");

            if (request.Password != request.ConfirmPassword)
                throw new ArgumentException("Passwords do not match.");

            var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser is not null)
                throw new InvalidOperationException("A user with this email already exists.");

            var user = new AppUser
            {
                Email = request.Email.Trim().ToLowerInvariant(),
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = "Customer",
                IsActive = true
            };

            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return new RegisterResponse
            {
                UserId = user.Id.ToString(),
                Email = user.Email,
                Role = user.Role,
                Success = true,
                Message = "Registration successful."
            };
        }
    }
}
