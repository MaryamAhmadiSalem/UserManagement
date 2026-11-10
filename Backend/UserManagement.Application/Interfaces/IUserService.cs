using UserManagement.Application.DTO;
using UserManagement.Application.ResultClass;
using UserManagement.Domain;

namespace UserManagement.Application.Interfaces
{
    public interface IUserService
    {
        public Task<ResultOfT<UserInformation>> CreateUser(CreateUserDto dto, string userId);
        public Task<ResultOfT<List<UserInformation>>> ReadAllUser();
        public Task<ResultOfT<UserInformation?>> ReadOneUser(int id, string userId);
        public Task<Result> UpdateUser(CreateUserDto dto, int id, string userId);
        public Task<Result> DeleteUser(int id, string userId);
        public Task<ResultOfT<UserInformation?>> GetByNationalId(string nationalId, string userId);
        public Task<ResultOfT<string>> Register(RegisterDto dto);
        public Task<ResultOfT<string>> Login(LoginDto dto);
    }
}