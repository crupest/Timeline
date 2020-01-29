using Microsoft.AspNetCore.Mvc;
using System;
using Timeline.Controllers;

namespace Timeline.Services
{
    public class User
    {
        public string? Username { get; set; }
        public string? Nickname { get; set; }
        public string? AvatarUrl { get; set; }

        #region adminsecret
        public bool? Administrator { get; set; }
        #endregion adminsecret

        #region secret
        public long? Id { get; set; }
        public string? Password { get; set; }
        public long? Version { get; set; }
        #endregion secret
    }

    public static class UserExtensions
    {
        public static User EraseSecretAndFinalFill(this User user, IUrlHelper urlHelper, bool adminstrator)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var result = new User
            {
                Username = user.Username,
                Nickname = user.Nickname,
                AvatarUrl = urlHelper.ActionLink(action: nameof(UserAvatarController.Get), controller: nameof(UserAvatarController), values: new
                {
                    user.Username
                })
            };

            if (adminstrator)
            {
                result.Administrator = user.Administrator;
            }

            return result;
        }
    }
}
