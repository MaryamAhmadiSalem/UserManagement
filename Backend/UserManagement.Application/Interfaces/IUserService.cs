using UserManagement.Application.DTO;
using UserManagement.Application.ResultClass;
using UserManagement.Domain;

namespace UserManagement.Application.Interfaces
{
    public interface IUserService
    {
        public Task<ResultOfT<UserInformation>> CreateUser(CreateUserDto dto, string userId);
        public Task<ResultOfT<List<UserListDto>>> ReadAllUser(string userId);
        public Task<ResultOfT<UserInformation?>> ReadOneUser(int id, string userId);
        public Task<Result> UpdateUser(CreateUserDto dto, int id, string userId);
        public Task<Result> DeleteUser(int id, string userId);
        public Task<ResultOfT<UserListDto?>> GetByNationalId(string nationalId, string userId);
        public Task<ResultOfT<AuthTokenDto>> Register(RegisterDto dto);
        public Task<ResultOfT<AuthTokenDto>> Login(LoginDto dto);
        public Task<Result> AssignRole(AssignRoleDto dto);
        public Task<ResultOfT<AuthTokenDto>> TokenRenewal(RefreshTokenDto dto);
        public Task<bool> TokenRevocation(RefreshTokenDto dto);

    }
}