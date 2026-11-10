using AutoMapper;
using UserManagement.Application.DTO;
using UserManagement.Domain;
using Microsoft.AspNetCore.Identity;

namespace UserManagement.Application.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<CreateUserDto, UserInformation>();
            CreateMap<RegisterDto, IdentityUser>()
                .ForMember(u => u.UserName, opt => opt.MapFrom(u => u.Email));
        }
    }
}
