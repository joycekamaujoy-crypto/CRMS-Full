using CRMS_API.Services.Interfaces;
using CRMS_API.Domain.DTOs;
using CRMS_API.Domain.Entities;
using CRMS_API.Domain.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CRMS_API.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IJwtGenerator _jwtGenerator;

        public AuthService(AppDbContext context, IPasswordHasher<User> passwordHasher, IConfiguration configuration, IJwtGenerator jwtGenerator)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request)
        {
            var existingUser = await _context.Users.AnyAsync(u => u.Email == request.Email);
            if (existingUser)
            {
                return null;
            }

            var newUser = new User
            {
                Name = request.Name,
                Email = request.Email,
                Role = request.Role
            };

            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, request.Password);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var token = _jwtGenerator.GenerateJwtToken(newUser.Id, newUser.Email, newUser.Role.ToString());

            return new AuthResponseDto
            {
                userId = newUser.Id,
                Name = newUser.Name,
                Email = newUser.Email,
                Role = newUser.Role.ToString(),
                Token = token
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return null;
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return null;
            }

            var token = _jwtGenerator.GenerateJwtToken(user.Id, user.Email, user.Role.ToString());

            return new AuthResponseDto
            {
                userId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                Token = token
            };
        }
    }
}
