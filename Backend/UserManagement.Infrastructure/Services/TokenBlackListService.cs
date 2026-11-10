using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Interfaces;
using UserManagement.Domain;
namespace UserManagement.Infrastructure.Services
{
    public class TokenBlackListService : ITokenBlackListService
    {
        private readonly AppDbContext _db;
        public TokenBlackListService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<bool> IsTokenBlackList(string jti)
        {
            var isTokenExist = await _db.TokenExpired.AnyAsync(x => x.Jti == jti);
            return isTokenExist;
        }
        public async Task<bool> AddTokenToDatabase(string jti, DateTimeOffset exp)
        {
            var tokenBlackList = new TokenInfoExpired
            {
                Jti = jti,
                Expires = exp,
            };
            _db.Add(tokenBlackList);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
