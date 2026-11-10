using AutoMapper;
using Microsoft.AspNetCore.Identity;
using UserManagement.Application.DTO;
using UserManagement.Application.Interfaces;
using UserManagement.Application.ResultClass;
using UserManagement.Application.Validators;
using UserManagement.Domain;

namespace UserManagement.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly UserModelValidator _validationsUserModel;
        private readonly RegisterValidator _validationsRegister;
        private readonly LoginValidator _validationsLogin;
        private readonly ITokenService _tokenService;

        public UserService(UserManager<IdentityUser> userManager,
            IUserRepository userRepository, IMapper mapper,
            UserModelValidator validationsUserModel, RegisterValidator validationsRegister,
            LoginValidator validationsLogin, ITokenService tokenService
            )
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _mapper = mapper;
            _validationsUserModel = validationsUserModel;
            _validationsRegister = validationsRegister;
            _validationsLogin = validationsLogin;
            _tokenService = tokenService;
        }
        public async Task<ResultOfT<UserInformation>> CreateUser(CreateUserDto dto, string userId)
        {
            var validationsResult = await _validationsUserModel.ValidateAsync(dto);
            if (!validationsResult.IsValid)
            {
                return UnsuccessfulResult<UserInformation>(
                     validationsResult.Errors.Select(e => e.ErrorMessage).ToList()
                );
            }
            var duplicateResult = await this.CheckingDuplicates(dto.NationalId, dto.Email, null);
            if (!duplicateResult.IsSuccess)
                return UnsuccessfulResult<UserInformation>(duplicateResult.Error!);
            var user = _mapper.Map<UserInformation>(dto);
            user.UserId = userId;
            var duplicateAccount = await _userRepository.ExistsByUserId(userId);
            if (!duplicateAccount)
            {
                var createUser = await _userRepository.Add(user);
                return SuccessfulResult(createUser);
            }
            return UnsuccessfulResult<UserInformation>(["ExsistAccount"]);
        }
        public async Task<ResultOfT<List<UserInformation>>> ReadAllUser()
        {
            var userInfo = await _userRepository.GetAll();
            return SuccessfulResult<List<UserInformation>>(userInfo);
        }
        public async Task<ResultOfT<UserInformation>> ReadOneUser(int id, string userId)
        {
            var user = await _userRepository.GetById(id);
            if (user == null) return UnsuccessfulResult<UserInformation>(["NotFound"]);
            if (user.UserId != userId) return UnsuccessfulResult<UserInformation>(["Forbidden"]);
            return SuccessfulResult(user);
        }
        public async Task<Result> UpdateUser(CreateUserDto dto, int id, string userId)
        {
            var user = await _userRepository.GetById(id);
            if (user == null) return Unsuccessful(["NotFound"]);

            if (userId != user.UserId)
                return Unsuccessful(["Forbidden"]);
            var validationResult = await _validationsUserModel.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Unsuccessful(
                    validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                );
            var duplicateResult = await this.CheckingDuplicates(dto.NationalId, dto.Email, id);
            if (!duplicateResult.IsSuccess)
                return Unsuccessful(duplicateResult.Error!);

            _mapper.Map(dto, user);
            await _userRepository.Update(user);

            return Successful();
        }
        public async Task<Result> DeleteUser(int id, string userId)
        {
            var user = await _userRepository.GetById(id);
            if (user == null)
                return Unsuccessful(["NotFound"]);
            if (user.UserId != userId)
                return Unsuccessful(["Forbidden"]);
            var identityUser = await _userManager.FindByIdAsync(user.UserId);
            if (identityUser == null)
                return Unsuccessful(["AccountNotFound"]);
            await _userRepository.Delete(user);
            var deletedAccount = await _userManager.DeleteAsync(identityUser);
            if (!deletedAccount.Succeeded)
                return Unsuccessful(["AccountDeletionFailed"]);
            return Successful();
        }
        public async Task<ResultOfT<UserInformation>> GetByNationalId(string nationalId, string userId)
        {
            var user = await _userRepository.GetByNationalId(nationalId);
            if (user == null)
                return UnsuccessfulResult<UserInformation>(["NotFound"]);
            if (user.UserId != userId)
                return UnsuccessfulResult<UserInformation>(["Forbidden"]);
            return SuccessfulResult(user);
        }
        public async Task<ResultOfT<string>> Register(RegisterDto dto)
        {
            var validationResult = await _validationsRegister.ValidateAsync(dto);
            if (validationResult.IsValid)
            {
                var user = _mapper.Map<IdentityUser>(dto);
                var createAccount = await _userManager.CreateAsync(user, dto.Password);
                if (!createAccount.Succeeded) return UnsuccessfulResult<string>(["RegisterFailed"]);
                var token = _tokenService.GenerateToken(user.Id);
                return SuccessfulResult<string>(token);
            }
            return UnsuccessfulResult<string>(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList()
            );
        }
        public async Task<ResultOfT<string>> Login(LoginDto dto)
        {
            await this.ValidationLogin(dto);
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return UnsuccessfulResult<string>(["NotFound"]);
            var checkPassword = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!checkPassword)
                return UnsuccessfulResult<string>(["IncorrectPassword"]);
            var token = _tokenService.GenerateToken(user.Id);
            return SuccessfulResult<string>(token);
        }

        private async Task<Result> CheckingDuplicates(string nationalId, string email, int? id)
        {
            var duplicateNationalId = await _userRepository.ExistsByNationalId(nationalId, id);
            var duplicateEmail = await _userRepository.ExistsByEmail(email, id);

            if (duplicateNationalId && duplicateEmail)
                return Unsuccessful(["NationalIdAndEmail"]);

            if (duplicateNationalId)
                return Unsuccessful(["NationalId"]);

            if (duplicateEmail)
                return Unsuccessful(["Email"]);

            return Successful();
        }
        private async Task<Result> ValidationLogin(LoginDto dto)
        {
            var validationResult = await _validationsLogin.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Unsuccessful(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            return Successful();
        }
        private ResultOfT<T> UnsuccessfulResult<T>(List<string> error)
        {
            return new ResultOfT<T>
            {
                IsSuccess = false,
                Error = error
            };
        }
        private ResultOfT<T> SuccessfulResult<T>(T data)
        {
            return new ResultOfT<T>
            {
                IsSuccess = true,
                Data = data
            };
        }
        private Result Unsuccessful(List<string> error)
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