using AutoMapper;
using System.ComponentModel.DataAnnotations;
using TimelineApp.Models.Validation;
using TimelineApp.Services;

namespace TimelineApp.Models.Http
{
    public class UserPatchRequest
    {
        [Username]
        public string? Username { get; set; }

        [MinLength(1)]
        public string? Password { get; set; }

        [Nickname]
        public string? Nickname { get; set; }

        public bool? Administrator { get; set; }
    }

    public class CreateUserRequest
    {
        [Required, Username]
        public string Username { get; set; } = default!;

        [Required, MinLength(1)]
        public string Password { get; set; } = default!;

        [Required]
        public bool? Administrator { get; set; }

        [Nickname]
        public string? Nickname { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string OldPassword { get; set; } = default!;
        [Required(AllowEmptyStrings = false)]
        public string NewPassword { get; set; } = default!;
    }

    public class UserControllerAutoMapperProfile : Profile
    {
        public UserControllerAutoMapperProfile()
        {
            CreateMap<UserPatchRequest, User>(MemberList.Source);
            CreateMap<CreateUserRequest, User>(MemberList.Source);
        }
    }
}
