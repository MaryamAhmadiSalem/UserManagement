using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Interfaces;
using UserManagement.Domain;

namespace UserManagement.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<UserInformation> Add(UserInformation userInfo)
        {
            _db.Users.Add(userInfo);
            await _db.SaveChangesAsync();
            return userInfo;
        }
        public Task<List<UserInformation>> GetAll()
        {
            return _db.Users.AsNoTracking().ToListAsync();
        }

        public Task<UserInformation?> GetById(int id)
        {
            return _db.Users.SingleOrDefaultAsync(u => u.Id == id);
        }

        public Task<UserInformation?> GetByNationalId(string nationalId)
        {
            return _db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.NationalId == nationalId);
        }

        public async Task<UserInformation> Update(UserInformation userInfo)
        {
            await _db.SaveChangesAsync();
            return userInfo;
        }
        public async Task Delete(UserInformation user)
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }
        public Task<bool> ExistsByNationalId(string nationalId, int? id)
        {
            return _db.Users.AnyAsync(u =>
            u.NationalId == nationalId &&
            (id == null || u.Id != id));
        }
        public Task<bool> ExistsByEmail(string email, int? id)
        {
            return _db.Users.AnyAsync(u =>
            u.Email == email &&
            (id == null || u.Id != id));
        }
        public Task<bool> ExistsByUserId(string userId)
        {
            return _db.Users.AnyAsync(u => u.UserId == userId);
        }
    }
}