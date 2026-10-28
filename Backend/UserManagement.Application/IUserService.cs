using UserManagement.Domain;

namespace UserManagement.Application
{
    public interface IUserService
    {
        public Task<ResultOfT<UserInformation>> CreateUser(CreateUserDto dto);
        public Task<ResultOfT<List<UserInformation>>> ReadAllUser();
        public Task<ResultOfT<UserInformation?>> ReadOneUser(int id);
        public Task<Result> UpdateUser(CreateUserDto dto, int id);
        public Task<Result> DeleteUser(int id);
        public Task<ResultOfT<UserInformation?>> GetByNationalId(string nationalId);
    }
}