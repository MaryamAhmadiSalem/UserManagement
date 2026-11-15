using System.Security.Claims;

namespace UserManagement.Application.Interfaces
{
    public interface ITokenService
    {
        public string GenerateToken(string userId, IEnumerable<Claim> claims);
        public string GenerateRefreshToken();
        public string HashRefreshToken(string token);
    }
}