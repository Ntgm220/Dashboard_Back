using Dashboard.Data.Entities;
using Dashboard.Data.DashboardDbContext;
using Dashboard.Infrastructure.DTOs;
using Dashboard.Infrastructure.Interfaces;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Dashboard.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly DashboardDbContext _db;
        private readonly IConfiguration _configuration;

        public AuthService(DashboardDbContext context, IConfiguration configuration)
        {
            _db = context;
            _configuration = configuration;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var email = request.Email.Trim();

            var user = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == email);

            if (user == null || !IsActiveEstado(user.Estado))
                return null;

            bool passwordValida = VerifyPassword(request.Password, user.Clave);

            if (!passwordValida)
                return null;

            if (!IsBcryptHash(user.Clave))
            {
                user.Clave = BCrypt.Net.BCrypt.HashPassword(request.Password);
                await _db.SaveChangesAsync();
            }

            var jwt = GenerateJwtToken(user);

            return new LoginResponse
            {
                Token = jwt.Token,
                Email = user.Correo,
                Nombre = user.Nombre,
                Rol = user.Rol,
                EmpresaId = user.EmpresaId,
                ExpiresAt = jwt.ExpiresAt
            };
        }

        private (string Token, DateTime ExpiresAt) GenerateJwtToken(Usuarios user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Correo),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Correo),
                new Claim(ClaimTypes.Name, user.Nombre),
                new Claim(ClaimTypes.Role, user.Rol),
                new Claim("empresa_id", user.EmpresaId.ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresMinutes = int.TryParse(_configuration["Jwt:ExpiresMinutes"], out var configuredMinutes)
                ? configuredMinutes
                : 120;
            var expiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }

        private static bool VerifyPassword(string plainPassword, string storedPassword)
        {
            if (string.IsNullOrWhiteSpace(storedPassword))
                return false;

            if (!IsBcryptHash(storedPassword))
                return string.Equals(plainPassword, storedPassword, StringComparison.Ordinal);

            try
            {
                return BCrypt.Net.BCrypt.Verify(plainPassword, storedPassword);
            }
            catch (SaltParseException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static bool IsBcryptHash(string value)
        {
            return value.StartsWith("$2a$", StringComparison.Ordinal)
                || value.StartsWith("$2b$", StringComparison.Ordinal)
                || value.StartsWith("$2y$", StringComparison.Ordinal);
        }

        private static bool IsActiveEstado(string? estado)
        {
            return string.Equals(estado?.Trim(), "activo", StringComparison.OrdinalIgnoreCase);
        }
    }
}
