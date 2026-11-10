using UserManagement.Domain;

namespace UserManagement.Application.Interfaces
{
    public interface IUserRepository
    {
        public Task<List<UserInformation>> GetAll();
        public Task<UserInformation?> GetById(int id);
        public Task<UserInformation?> GetByNationalId(string nationalId);
        public Task<UserInformation> Add(UserInformation userInfo);
        public Task<UserInformation> Update(UserInformation userInfo);
        public Task Delete(UserInformation user);
        public Task<bool> ExistsByNationalId(string nationalId, int? id);
        public Task<bool> ExistsByEmail(string email, int? id);
        public Task<bool> ExistsByUserId(string userId);
    }
}