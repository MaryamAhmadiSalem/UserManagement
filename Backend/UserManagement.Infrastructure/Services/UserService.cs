using System.Security.Claims;
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
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(UserManager<IdentityUser> userManager,
            IUserRepository userRepository, IMapper mapper,
            UserModelValidator validationsUserModel, RegisterValidator validationsRegister,
            LoginValidator validationsLogin, ITokenService tokenService, RoleManager<IdentityRole> roleManager
            )
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _mapper = mapper;
            _validationsUserModel = validationsUserModel;
            _validationsRegister = validationsRegister;
            _validationsLogin = validationsLogin;
            _tokenService = tokenService;
            _roleManager = roleManager;
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
        public async Task<ResultOfT<List<UserListDto>>> ReadAllUser(string userId)
        {
            var usersInfo = await _userRepository.GetAll();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return UnsuccessfulResult<List<UserListDto>>(["NotFound"]);
            var roles = await _userManager.GetRolesAsync(user!);

            if (!roles.Contains("Admin") && !roles.Contains("Manager"))
                return UnsuccessfulResult<List<UserListDto>>(["Fail"]); 
            var result = new List<UserListDto>();
            foreach (var userInfo in usersInfo)
            {
                var identityUser = await _userManager.FindByIdAsync(userInfo.UserId);
                if (identityUser == null)
                    continue;
                var userRoles = await _userManager.GetRolesAsync(identityUser);
                if (roles.Contains("Admin") && !userRoles.Contains("User"))
                    continue;
                var userDto = _mapper.Map<UserListDto>(userInfo);
                userDto.Role = userRoles.FirstOrDefault() ?? "Unknown";
                result.Add(userDto);
            }
            return SuccessfulResult<List<UserListDto>>(result);
        }
        public async Task<ResultOfT<UserInformation>> ReadOneUser(int id, string userId)
        {
            var targetUser = await _userRepository.GetById(id);
            if (targetUser == null) return UnsuccessfulResult<UserInformation>(["NotFound"]);

            var permission = await CanManageUser(targetUser, userId);
            if (!permission.IsSuccess)
                return UnsuccessfulResult<UserInformation>(permission.Error!);

            return SuccessfulResult(targetUser);
        }
        public async Task<Result> UpdateUser(CreateUserDto dto, int id, string userId)
        {
            var validationResult = await _validationsUserModel.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Unsuccessful(
                      validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                );
            var duplicateResult = await this.CheckingDuplicates(dto.NationalId, dto.Email, id);
            if (!duplicateResult.IsSuccess)
                return Unsuccessful(duplicateResult.Error!);

            var targetUser = await _userRepository.GetById(id);
            if (targetUser == null) return Unsuccessful(["NotFound"]);

            var permission = await CanManageUser(targetUser, userId);
            if (!permission.IsSuccess)
                return Unsuccessful(permission.Error!);
            _mapper.Map(dto, targetUser);
            await _userRepository.Update(targetUser);
            return Successful();
        }
        public async Task<Result> DeleteUser(int id, string userId)
        {
            var targetUser = await _userRepository.GetById(id);
            if (targetUser == null)
                return Unsuccessful(["NotFound"]);
            var permission = await CanManageUser(targetUser, userId);
            if (!permission.IsSuccess)
                return Unsuccessful(permission.Error!);

            var identityUser = await _userManager.FindByIdAsync(targetUser.UserId);
            if (identityUser == null)
                return Unsuccessful(["AccountNotFound"]);

            var deletedAccount = await _userManager.DeleteAsync(identityUser);
            if (!deletedAccount.Succeeded)
                return Unsuccessful(["AccountDeletionFailed"]);

            //await _userRepository.Delete(targetUser);
            return Successful();
        }
        public async Task<ResultOfT<UserListDto>> GetByNationalId(string nationalId, string userId)
        {
            var targetUser = await _userRepository.GetByNationalId(nationalId);
            if (targetUser == null)
                return UnsuccessfulResult<UserListDto>(["NotFound"]);

            var permission = await CanManageUser(targetUser, userId);
            if (!permission.IsSuccess)
                return UnsuccessfulResult<UserListDto>(permission.Error!);

            var identityUser = await _userManager.FindByIdAsync(targetUser.UserId);

            if (identityUser == null)
                return UnsuccessfulResult<UserListDto>(["AccountNotFound"]);

            var roles = await _userManager.GetRolesAsync(identityUser);

            var userDto = _mapper.Map<UserListDto>(targetUser);
            userDto.Role = roles.FirstOrDefault() ?? "Unknown";

            return SuccessfulResult(userDto);
        }
        public async Task<ResultOfT<AuthTokenDto>> Register(RegisterDto dto)
        {
            var validationResult = await _validationsRegister.ValidateAsync(dto);
            if (validationResult.IsValid)
            {
                var user = _mapper.Map<IdentityUser>(dto);
                var createAccount = await _userManager.CreateAsync(user, dto.Password);
                if (!createAccount.Succeeded) return UnsuccessfulResult<AuthTokenDto>(["RegisterFailed"]);

                var addRole = await _userManager.AddToRoleAsync(user, "User");
                if (!addRole.Succeeded)
                    return UnsuccessfulResult<AuthTokenDto>(["RoleAssignmentFailed"]);

                var role = await _roleManager.FindByNameAsync("User");
                var claims = await _roleManager.GetClaimsAsync(role!);
                claims.Add(new Claim(ClaimTypes.Role, "User"));

                var accessToken = _tokenService.GenerateToken(user.Id, claims);
                return await RefreshToken(user.Id, accessToken);
            }
            return UnsuccessfulResult<AuthTokenDto>(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList()
            );
        }
        public async Task<ResultOfT<AuthTokenDto>> Login(LoginDto dto)
        {
            var validationResult = await ValidationLogin(dto);
            if (!validationResult.IsSuccess)
                return UnsuccessfulResult<AuthTokenDto>(validationResult.Error!);

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return UnsuccessfulResult<AuthTokenDto>(["NotFound"]);
            var checkPassword = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!checkPassword)
                return UnsuccessfulResult<AuthTokenDto>(["IncorrectPassword"]);
            var roleClaims = new List<Claim>();
            var rolesUser = await _userManager.GetRolesAsync(user);
            foreach (var roleName in rolesUser)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                var claims = await _roleManager.GetClaimsAsync(role!);
                roleClaims.Add(new Claim(ClaimTypes.Role, roleName));
                roleClaims.AddRange(claims);
            }
            var accessToken = _tokenService.GenerateToken(user.Id, roleClaims);
            return await RefreshToken(user.Id, accessToken);
        }
        public async Task<Result> AssignRole(AssignRoleDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null) return Unsuccessful(["NotFound"]);
            var existRole = await _roleManager.RoleExistsAsync(dto.RoleName);
            if (!existRole) return Unsuccessful(["RoleNotFound"]);
            var roles = await _userManager.GetRolesAsync(user);
            if (dto.RoleName != "Admin" && dto.RoleName != "User")
                return Unsuccessful(["InvalidRole"]);
            if (roles.Contains("Manager")) return Unsuccessful(["CannotChangeManagerRole"]);
            var removeRoles = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!removeRoles.Succeeded) return Unsuccessful(["RemovingFailed"]);
            var addNewRole = await _userManager.AddToRoleAsync(user, dto.RoleName);
            if (!addNewRole.Succeeded) return Unsuccessful(["AddingFailed"]);
            return Successful();
        }
        public async Task<ResultOfT<AuthTokenDto>> TokenRenewal(RefreshTokenDto dto)
        {
            var token = _tokenService.HashRefreshToken(dto.Token);
            var refreshToken = await _userRepository.ExistRefreshToken(token);
            if (refreshToken == null) return UnsuccessfulResult<AuthTokenDto>(["Invalid"]);
            else if (refreshToken.Revoked != null) return UnsuccessfulResult<AuthTokenDto>(["Revoked"]);
            else if (refreshToken.Expires <= DateTimeOffset.UtcNow) return UnsuccessfulResult<AuthTokenDto>(["Expired"]);

            var tokenOwner = await _userManager.FindByIdAsync(refreshToken.UserId);
            if (tokenOwner == null) return UnsuccessfulResult<AuthTokenDto>(["NotFound"]);
            var roles = await _userManager.GetRolesAsync(tokenOwner!);
            var claims = new List<Claim>();
            foreach (var role in roles)
            {
                var roleUser = await _roleManager.FindByNameAsync(role);
                var calimsRole = await _roleManager.GetClaimsAsync(roleUser!);
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.AddRange(calimsRole);
            }
            await _userRepository.RevokeRefreshToken(refreshToken.Token);
            var newAccessToken = _tokenService.GenerateToken(refreshToken.UserId, claims);
            return await RefreshToken(refreshToken.UserId, newAccessToken);
        }
        public async Task<bool> TokenRevocation(RefreshTokenDto dto)
        {
            var hashToken = _tokenService.HashRefreshToken(dto.Token);
            var refreshToken = await _userRepository.ExistRefreshToken(hashToken);
            if (refreshToken == null || refreshToken.Revoked != null || refreshToken.Expires <= DateTimeOffset.UtcNow) return false;
            await _userRepository.RevokeRefreshToken(hashToken);
            return true;
        }


        private async Task<Result> CanManageUser(UserInformation targetUser, string userId)
        {
            var currentUser = await _userManager.FindByIdAsync(userId);
            if (currentUser == null)
                return Unsuccessful(["NotFound"]);

            var currentRoles = await _userManager.GetRolesAsync(currentUser);

            if (currentRoles.Contains("Manager"))
                return Successful();
            if (currentRoles.Contains("Admin"))
            {
                var targetAccount = await _userManager.FindByIdAsync(targetUser.UserId);
                if (targetAccount == null)
                    return Unsuccessful(["AccountNotFound"]);
                var targetRoles = await _userManager.GetRolesAsync(targetAccount);
                if (targetRoles.Contains("User"))
                    return Successful();
                return Unsuccessful(["Forbidden"]);
            }
            if (currentRoles.Contains("User"))
            {
                if (targetUser.UserId == userId)
                    return Successful();
                return Unsuccessful(["Forbidden"]);
            }
            return Unsuccessful(["Forbidden"]);
        }
        private async Task<ResultOfT<AuthTokenDto>> RefreshToken(string userId, string accessToken)
        {
            var refreshToken = _tokenService.GenerateRefreshToken();
            var hashRefreshToken = _tokenService.HashRefreshToken(refreshToken);

            var refreshTokenEntity = new RefreshToken
            {
                Token = hashRefreshToken,
                UserId = userId,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Revoked = null
            };
            await _userRepository.AddRefreshToken(refreshTokenEntity);

            var tokenDto = new AuthTokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            return SuccessfulResult<AuthTokenDto>(tokenDto);
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