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

    public enum CreateUserResult
    {
        Success,
        AlreadyExists
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

        Task<CreateUserResult> CreateUser(string username, string password, string[] roles);
    }

    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly DatabaseContext _databaseContext;
        private readonly IJwtService _jwtService;
        private readonly IPasswordService _passwordService;

        public UserService(ILogger<UserService> logger, DatabaseContext databaseContext, IJwtService jwtService, IPasswordService passwordService)
        {
            _logger = logger;
            _databaseContext = databaseContext;
            _jwtService = jwtService;
            _passwordService = passwordService;
        }

        public async Task<CreateTokenResult> CreateToken(string username, string password)
        {
            var users = _databaseContext.Users.ToList();

            var user = await _databaseContext.Users.Where(u => u.Name == username).SingleOrDefaultAsync();

            if (user == null)
            {
                _logger.LogInformation($"Create token failed with invalid username. Username = {username} Password = {password} .");
                return null;
            }

            var verifyResult = _passwordService.VerifyPassword(user.EncryptedPassword, password);

            if (verifyResult)
            {
                var userInfo = new UserInfo(user);

                return new CreateTokenResult
                {
                    Token = _jwtService.GenerateJwtToken(user.Id, userInfo.Roles),
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
            var userId = _jwtService.VerifyJwtToken(token);

            if (userId == null)
            {
                _logger.LogInformation($"Verify token falied. Reason: invalid token. Token: {token} .");
                return null;
            }

            var user = await _databaseContext.Users.Where(u => u.Id == userId.Value).SingleOrDefaultAsync();

            if (user == null)
            {
                _logger.LogInformation($"Verify token falied. Reason: invalid user id. UserId: {userId} Token: {token} .");
                return null;
            }

            return new UserInfo(user);
        }

        public async Task<CreateUserResult> CreateUser(string username, string password, string[] roles)
        {
            var exists = (await _databaseContext.Users.Where(u => u.Name == username).ToListAsync()).Count != 0;

            if (exists)
            {
                return CreateUserResult.AlreadyExists;
            }

            await _databaseContext.Users.AddAsync(new User { Name = username, EncryptedPassword = _passwordService.HashPassword(password), RoleString = string.Join(',', roles) });
            await _databaseContext.SaveChangesAsync();

            return CreateUserResult.Success;
        }
    }
}
