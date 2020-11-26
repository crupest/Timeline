using AutoMapper;
using System.ComponentModel.DataAnnotations;
using Timeline.Controllers;
using Timeline.Models.Validation;
using Timeline.Services;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Request model for <see cref="UserController.Patch(HttpUserPatchRequest, string)"/>.
    /// </summary>
    public class HttpUserPatchRequest
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
    }

    /// <summary>
    /// Request model for <see cref="UserController.CreateUser(HttpCreateUserRequest)"/>.
    /// </summary>
    public class HttpCreateUserRequest
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
    }

    /// <summary>
    /// Request model for <see cref="UserController.ChangePassword(HttpChangePasswordRequest)"/>.
    /// </summary>
    public class HttpChangePasswordRequest
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

    public class HttpUserControllerModelAutoMapperProfile : Profile
    {
        public HttpUserControllerModelAutoMapperProfile()
        {
            CreateMap<HttpUserPatchRequest, ModifyUserParams>();
        }
    }
}
