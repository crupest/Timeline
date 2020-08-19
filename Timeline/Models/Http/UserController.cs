using AutoMapper;
using System.ComponentModel.DataAnnotations;
using Timeline.Controllers;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Request model for <see cref="UserController.Patch(UserPatchRequest, string)"/>.
    /// </summary>
    public class UserPatchRequest
    {
        /// <summary>
        /// New username. Null if not change. Need to be administrator.
        /// </summary>
        [Username]
        public string? Username { get; set; }

        /// <summary>
        /// New password. Null if not change. Need to be administrator.
        /// </summary>
        [MinLength(1)]
        public string? Password { get; set; }

        /// <summary>
        /// New nickname. Null if not change. Need to be administrator to change other's.
        /// </summary>
        [Nickname]
        public string? Nickname { get; set; }

        /// <summary>
        /// Whether to be administrator. Null if not change. Need to be administrator.
        /// </summary>
        public bool? Administrator { get; set; }
    }

    /// <summary>
    /// Request model for <see cref="UserController.CreateUser(CreateUserRequest)"/>.
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// Username of the new user.
        /// </summary>
        [Required, Username]
        public string Username { get; set; } = default!;

        /// <summary>
        /// Password of the new user.
        /// </summary>
        [Required, MinLength(1)]
        public string Password { get; set; } = default!;

        /// <summary>
        /// Whether the new user is administrator.
        /// </summary>
        [Required]
        public bool? Administrator { get; set; }

        /// <summary>
        /// Nickname of the new user.
        /// </summary>
        [Nickname]
        public string? Nickname { get; set; }
    }

    /// <summary>
    /// Request model for <see cref="UserController.ChangePassword(ChangePasswordRequest)"/>.
    /// </summary>
    public class ChangePasswordRequest
    {
        /// <summary>
        /// Old password.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string OldPassword { get; set; } = default!;

        /// <summary>
        /// New password.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string NewPassword { get; set; } = default!;
    }

    /// <summary>
    /// 
    /// </summary>
    public class UserControllerAutoMapperProfile : Profile
    {
        /// <summary>
        /// 
        /// </summary>
        public UserControllerAutoMapperProfile()
        {
            CreateMap<UserPatchRequest, User>(MemberList.Source);
            CreateMap<CreateUserRequest, User>(MemberList.Source);
        }
    }
}
