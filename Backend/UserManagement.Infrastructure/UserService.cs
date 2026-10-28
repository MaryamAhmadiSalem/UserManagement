using UserManagement.Application;
using UserManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.Infrastructure 
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        public UserService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ResultOfT<UserInformation>> CreateUser(CreateUserDto dto)
        {
            var duplicateNationalId = await _db.Users.AnyAsync(d => d.NationalId == dto.NationalId);
            var duplicateEmail = await _db.Users.AnyAsync(d => d.Email == dto.Email);

            if (duplicateNationalId && duplicateEmail)
                return UnsuccessfulResult("NationalIdAndEmail");

            if (duplicateNationalId)
                return UnsuccessfulResult("NationalId");

            if (duplicateEmail)
                return UnsuccessfulResult("Email");
            var user = new UserInformation
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                NationalId = dto.NationalId,
                DateOfBirth = dto.DateOfBirth,
                Job = dto.Job,
                Education = dto.Education,
                Email = dto.Email,
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return SuccessfulResult(user);
        }
        public async Task<ResultOfT<List<UserInformation>>> ReadAllUser()
        {
            //List<UserInformation> userInfo = await _db.Users.ToListAsync();
            //userInfo[0].FirstName = "Reza";

            List<UserInformation> userInfo = await _db.Users.AsNoTracking().ToListAsync();
            return new ResultOfT<List<UserInformation>>
            {
                IsSuccess = true,
                Data = userInfo
            };
        }
        public async Task<ResultOfT<UserInformation?>> ReadOneUser(int id)
        {
            //var user1 = await _db.Users.SingleOrDefaultAsync(user => user.Id == id);

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == id);
            if (user != null)
                return SuccessfulResult(user);

            else
                return UnsuccessfulResult("NotFound");
        }
        public async Task<Result> UpdateUser(CreateUserDto dto, int id)
        {
            //var user2 = _db.Users.Where(user => user.Id == id).ToList();
            //user2[0].FirstName = dto.FirstName;
            //await _db.SaveChangesAsync();

            var user = await _db.Users.SingleOrDefaultAsync(user => user.Id == id);
            if (user == null)
                return Unsuccessful("NotFound");
            else
            {
                var duplicateNationalId = await _db.Users
                    .AnyAsync(d => d.NationalId == dto.NationalId && d.Id != id);
                var duplicateEmail = await _db.Users
                    .AnyAsync(d => d.Email == dto.Email && d.Id != id);

                if (duplicateNationalId && duplicateEmail)
                    return Unsuccessful("NationalIdAndEmail");
                if (duplicateNationalId)
                    return Unsuccessful("NationalId");
                if (duplicateEmail)
                    return Unsuccessful("Email");

                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.NationalId = dto.NationalId;
                user.DateOfBirth = dto.DateOfBirth;
                user.Job = dto.Job;
                user.Education = dto.Education;
                user.Email = dto.Email;
                await _db.SaveChangesAsync();

                return Successful();
            }
        }
        public async Task<Result> DeleteUser(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return Unsuccessful("NotFound");
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return Successful();
        }
        public async Task<ResultOfT<UserInformation?>> GetByNationalId(string nationalId)
        {   //Nested (where) are converted into (and)
            //var users = _db.Users.Where(t => t.Education == "ارشد");
            //var newUsers = users.Where(t => t.FirstName == "مریم").ToList();

            var user = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.NationalId == nationalId);
            if (user != null)
                return SuccessfulResult(user);
            else
                return UnsuccessfulResult("NotFound");
        }
        private ResultOfT<UserInformation> UnsuccessfulResult(string error)
        {
            return new ResultOfT<UserInformation>
            {
                IsSuccess = false,
                Error = error
            };
        }
        private ResultOfT<UserInformation> SuccessfulResult(UserInformation data)
        {
            return new ResultOfT<UserInformation>
            {
                IsSuccess = true,
                Data = data
            };
        }
        private Result Unsuccessful(string error)
        {
            return new Result
            {
                IsSuccess = false,
                Error = error
            };
        }
        private Result Successful()
        {
            return new Result
            {
                IsSuccess = true
            };
        }
    }
}