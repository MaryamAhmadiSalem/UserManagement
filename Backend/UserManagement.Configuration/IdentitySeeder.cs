using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UserManagement.Configuration.Authorization;

namespace UserManagement.Configuration
{
    public class IdentitySeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IOptions<InitialManagerSetting> _managerSetting;
        public IdentitySeeder(RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager,
            IOptions<InitialManagerSetting> initialManagerSetting)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _managerSetting = initialManagerSetting;
        }
        public async Task CreateRoles()
        {
            string[] roles = ["Manager", "Admin", "User"];
            foreach (var role in roles)
            {
                var existRole = await _roleManager.RoleExistsAsync(role);
                if (!existRole)
                {
                    var resultCreate = await _roleManager.CreateAsync(new IdentityRole(role));
                    if (!resultCreate.Succeeded)
                    {
                        var errors = string.Join(", ", resultCreate.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to create role {role}: {errors}");
                    }
                }
            }
        }
        public async Task CreateManager()
        {
            var existManager = await _userManager.FindByEmailAsync(_managerSetting.Value.Email);
            if (existManager == null)
            {
                var manager = new IdentityUser
                {
                    UserName = _managerSetting.Value.Email,
                    Email = _managerSetting.Value.Email
                };
                var createManager = await _userManager.CreateAsync(manager, _managerSetting.Value.Password);
                if (createManager.Succeeded)
                {
                    var addRole = await _userManager.AddToRoleAsync(manager, "Manager");
                    if (!addRole.Succeeded) throw new Exception($"Failed to add role : {addRole}");
                }
                else throw new Exception($"Failed to create role : {createManager}");
            }
        }
        public async Task CreateRoleClaim()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            foreach (var role in roles)
            {
                var permissions = role.Name switch
                {
                    "Manager" => new[]
                    {
                       UserPermissions.CreateUsers,
                       UserPermissions.ReadUsers,
                       UserPermissions.UpdateUsers,
                       UserPermissions.DeleteUsers,
                       UserPermissions.AssignRoles,
                    },
                    "Admin" => new[]
                    {
                        UserPermissions.CreateUsers,
                        UserPermissions.ReadUsers,
                        UserPermissions.UpdateUsers,
                        UserPermissions.DeleteUsers,
                    },
                    "User" => new[]
                    {
                        UserPermissions.ReadUsers,
                        UserPermissions.UpdateUsers,
                        UserPermissions.DeleteUsers
                    },
                    _ => Array.Empty<string>()
                };
                var existClaim = await _roleManager.GetClaimsAsync(role);
                foreach (var permission in permissions)
                {
                    if (!existClaim.Any(c => c.Type == "permission" && c.Value == permission))
                    {
                        var result = await _roleManager.AddClaimAsync(
                            role,
                            new Claim("permission", permission)
                        );
                        if (!result.Succeeded) throw new Exception($"Failed to add claim: {permission}");
                    }
                }
            }
        }
    }
}