using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace UserManagement.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly SigningCredentials _credentials;
        private readonly string _issuer;
        private readonly string _audience;
        public TokenService(IConfiguration configuration)
        {
            IConfiguration _configuration = configuration;
            var secretKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is missing.");
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var securityKey = new SymmetricSecurityKey(keyBytes);
            _credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            _issuer = _configuration["Jwt:Issuer"]!;
            _audience = _configuration["Jwt:Audience"]!;
        }
        public string GenerateToken(string userId)
        {
            var claim = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };
            var generateToken = new JwtSecurityToken(
               issuer: _issuer,
               audience: _audience,
               claims: claim,
               expires: DateTime.UtcNow.AddMinutes(30),
               signingCredentials: _credentials
            );
            var tokenHandeler = new JwtSecurityTokenHandler();
            return tokenHandeler.WriteToken(generateToken);
        }
    }
}