using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models;

namespace Timeline.Services
{
    public class CreateTokenResult
    {
        public string Token { get; set; }
        public UserInfo UserInfo { get; set; }
    }

    public enum PutUserResult
    {
        /// <summary>
        /// A new user is created.
        /// </summary>
        Created,
        /// <summary>
        /// A existing user is modified.
        /// </summary>
        Modified
    }

    public enum PatchUserResult
    {
        /// <summary>
        /// Succeed to modify user.
        /// </summary>
        Success,
        /// <summary>
        /// A user of given username does not exist.
        /// </summary>
        NotExists
    }

    public enum DeleteUserResult
    {
        /// <summary>
        /// A existing user is deleted.
        /// </summary>
        Deleted,
        /// <summary>
        /// A user of given username does not exist.
        /// </summary>
        NotExists
    }

    public enum ChangePasswordResult
    {
        /// <summary>
        /// Success to change password.
        /// </summary>
        Success,
        /// <summary>
        /// The user does not exists.
        /// </summary>
        NotExists,
        /// <summary>
        /// Old password is wrong.
        /// </summary>
        BadOldPassword
    }

    public interface IUserService
    {
        /// <summary>
        /// Try to anthenticate with the given username and password.
        /// If success, create a token and return the user info.
        /// </summary>
        /// <param name="username">The username of the user to be anthenticated.</param>
        /// <param name="password">The password of the user to be anthenticated.</param>
        /// <returns>Return null if anthentication failed. An <see cref="CreateTokenResult"/> containing the created token and user info if anthentication succeeded.</returns>
        Task<CreateTokenResult> CreateToken(string username, string password);

        /// <summary>
        /// Verify the given token.
        /// If success, return the user info.
        /// </summary>
        /// <param name="token">The token to verify.</param>
        /// <returns>Return null if verification failed. The user info if verification succeeded.</returns>
        Task<UserInfo> VerifyToken(string token);

        /// <summary>
        /// Get the user info of given username.
        /// </summary>
        /// <param name="username">Username of the user.</param>
        /// <returns>The info of the user. Null if the user of given username does not exists.</returns>
        Task<UserInfo> GetUser(string username);

        /// <summary>
        /// List all users.
        /// </summary>
        /// <returns>The user info of users.</returns>
        Task<UserInfo[]> ListUsers();

        /// <summary>
        /// Create or modify a user with given username.
        /// Return <see cref="PutUserResult.Created"/> if a new user is created.
        /// Return <see cref="PutUserResult.Modified"/> if a existing user is modified.
        /// </summary>
        /// <param name="username">Username of user.</param>
        /// <param name="password">Password of user.</param>
        /// <param name="roles">Array of roles of user.</param>
        /// <returns>Return <see cref="PutUserResult.Created"/> if a new user is created.
        /// Return <see cref="PutUserResult.Modified"/> if a existing user is modified.</returns>
        Task<PutUserResult> PutUser(string username, string password, string[] roles);

        /// <summary>
        /// Partially modify a use of given username.
        /// </summary>
        /// <param name="username">Username of the user to modify.</param>
        /// <param name="password">New password. If not modify, then null.</param>
        /// <param name="roles">New roles. If not modify, then null.</param>
        /// <returns>Return <see cref="PatchUserResult.Success"/> if modification succeeds.
        /// Return <see cref="PatchUserResult.NotExists"/> if the user of given username doesn't exist.</returns>
        Task<PatchUserResult> PatchUser(string username, string password, string[] roles);

        /// <summary>
        /// Delete a user of given username.
        /// Return <see cref="DeleteUserResult.Deleted"/> if the user is deleted.
        /// Return <see cref="DeleteUserResult.NotExists"/> if the user of given username
        /// does not exist.
        /// </summary>
        /// <param name="username">Username of thet user to delete.</param>
        /// <returns><see cref="DeleteUserResult.Deleted"/> if the user is deleted.
        /// <see cref="DeleteUserResult.NotExists"/> if the user doesn't exist.</returns>
        Task<DeleteUserResult> DeleteUser(string username);

        /// <summary>
        /// Try to change a user's password with old password.
        /// </summary>
        /// <param name="username">The name of user to change password of.</param>
        /// <param name="oldPassword">The user's old password.</param>
        /// <param name="newPassword">The user's new password.</param>
        /// <returns><see cref="ChangePasswordResult.Success"/> if success.
        /// <see cref="ChangePasswordResult.NotExists"/> if user does not exist.
        /// <see cref="ChangePasswordResult.BadOldPassword"/> if old password is wrong.</returns>
        Task<ChangePasswordResult> ChangePassword(string username, string oldPassword, string newPassword);

        Task<string> GetAvatarUrl(string username);
    }

    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly DatabaseContext _databaseContext;
        private readonly IJwtService _jwtService;
        private readonly IPasswordService _passwordService;
        private readonly ITencentCloudCosService _cosService;

        public UserService(ILogger<UserService> logger, DatabaseContext databaseContext, IJwtService jwtService, IPasswordService passwordService, ITencentCloudCosService cosService)
        {
            _logger = logger;
            _databaseContext = databaseContext;
            _jwtService = jwtService;
            _passwordService = passwordService;
            _cosService = cosService;
        }

        public async Task<CreateTokenResult> CreateToken(string username, string password)
        {
            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();

            if (user == null)
            {
                _logger.LogInformation($"Create token failed with invalid username. Username = {username} Password = {password} .");
                return null;
            }

            var verifyResult = _passwordService.VerifyPassword(user.EncryptedPassword, password);

            if (verifyResult)
            {
                var userInfo = UserInfo.Create(user);

                return new CreateTokenResult
                {
                    Token = _jwtService.GenerateJwtToken(user.Id, userInfo.Username, userInfo.Roles),
                    UserInfo = userInfo
                };
            }
            else
            {
                _logger.LogInformation($"Create token failed with invalid password. Username = {username} Password = {password} .");
                return null;
            }
        }

        public async Task<UserInfo> VerifyToken(string token)
        {
            var userInfo = _jwtService.VerifyJwtToken(token);

            if (userInfo == null)
            {
                _logger.LogInformation($"Verify token falied. Reason: invalid token. Token: {token} .");
                return null;
            }

            return await Task.FromResult(userInfo);
        }

        public async Task<UserInfo> GetUser(string username)
        {
            return await _databaseContext.Users
                .Where(user => user.Name == username)
                .Select(user => UserInfo.Create(user.Name, user.RoleString))
                .SingleOrDefaultAsync();
        }

        public async Task<UserInfo[]> ListUsers()
        {
            return await _databaseContext.Users
                .Select(user => UserInfo.Create(user.Name, user.RoleString))
                .ToArrayAsync();
        }

        public async Task<PutUserResult> PutUser(string username, string password, string[] roles)
        {
            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();

            if (user == null)
            {
                await _databaseContext.AddAsync(new User
                {
                    Name = username,
                    EncryptedPassword = _passwordService.HashPassword(password),
                    RoleString = string.Join(',', roles)
                });
                await _databaseContext.SaveChangesAsync();
                return PutUserResult.Created;
            }

            user.EncryptedPassword = _passwordService.HashPassword(password);
            user.RoleString = string.Join(',', roles);
            await _databaseContext.SaveChangesAsync();

            return PutUserResult.Modified;
        }

        public async Task<PatchUserResult> PatchUser(string username, string password, string[] roles)
        {
            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();

            if (user == null)
                return PatchUserResult.NotExists;

            bool modified = false;

            if (password != null)
            {
                modified = true;
                user.EncryptedPassword = _passwordService.HashPassword(password);
            }

            if (roles != null)
            {
                modified = true;
                user.RoleString = string.Join(',', roles);
            }

            if (modified)
            {
                await _databaseContext.SaveChangesAsync();
            }

            return PatchUserResult.Success;
        }

        public async Task<DeleteUserResult> DeleteUser(string username)
        {
            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();

            if (user == null)
            {
                return DeleteUserResult.NotExists;
            }

            _databaseContext.Users.Remove(user);
            await _databaseContext.SaveChangesAsync();
            return DeleteUserResult.Deleted;
        }

        public async Task<ChangePasswordResult> ChangePassword(string username, string oldPassword, string newPassword)
        {
            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();
            if (user == null)
                return ChangePasswordResult.NotExists;

            var verifyResult = _passwordService.VerifyPassword(user.EncryptedPassword, oldPassword);
            if (!verifyResult)
                return ChangePasswordResult.BadOldPassword;

            user.EncryptedPassword = _passwordService.HashPassword(newPassword);
            await _databaseContext.SaveChangesAsync();
            return ChangePasswordResult.Success;
        }

        public async Task<string> GetAvatarUrl(string username)
        {
            var exists = await _cosService.Exists("avatar", username);
            if (exists)
                return _cosService.GetObjectUrl("avatar", username);
            else
                return _cosService.GetObjectUrl("avatar", "__default");
        }
    }
}
