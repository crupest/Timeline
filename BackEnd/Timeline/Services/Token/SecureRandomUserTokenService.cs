using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Timeline.Configs;
using Timeline.Entities;

namespace Timeline.Services.Token
{
    public class SecureRandomUserTokenService : IUserTokenService, IDisposable
    {
        private DatabaseContext _databaseContext;
        private ILogger<SecureRandomUserTokenService> _logger;
        private RandomNumberGenerator _secureRandom;
        private IOptionsMonitor<TokenOptions> _optionMonitor;
        private IClock _clock;

        public SecureRandomUserTokenService(DatabaseContext databaseContext, ILogger<SecureRandomUserTokenService> logger, IOptionsMonitor<TokenOptions> optionMonitor, IClock clock)
        {
            _databaseContext = databaseContext;
            _logger = logger;
            _secureRandom = RandomNumberGenerator.Create();
            _optionMonitor = optionMonitor;
            _clock = clock;
        }

        public void Dispose()
        {
            _secureRandom.Dispose();
        }

        private string GenerateSecureRandomTokenString()
        {
            var option = _optionMonitor.CurrentValue;
            var tokenLength = option.TokenLength ?? 32;
            var buffer = new byte[tokenLength];
            _secureRandom.GetBytes(buffer);
            return Convert.ToHexString(buffer);
        }

        /// <inheritdoc/>
        public async Task<string> CreateTokenAsync(long userId, DateTime? expireTime)
        {
            var currentTime = _clock.GetCurrentTime();

            if (expireTime is not null && expireTime > currentTime)
            {
                _logger.LogWarning("The expire time of the token has already passed.");
            }

            UserTokenEntity entity = new UserTokenEntity
            {
                UserId = userId,
                Token = GenerateSecureRandomTokenString(),
                ExpireAt = expireTime,
                CreateAt = currentTime,
                Deleted = false
            };

            _databaseContext.UserTokens.Add(entity);
            await _databaseContext.SaveChangesAsync();

            _logger.LogInformation("A user token is created with user id {}.", userId);

            return entity.Token;
        }

        /// <inheritdoc/>
        public async Task<UserTokenInfo> ValidateTokenAsync(string token)
        {
            var entity = await _databaseContext.UserTokens.Where(t => t.Token == token && !t.Deleted).SingleOrDefaultAsync();

            if (entity is null)
            {
                throw new UserTokenException(token, Resource.ExceptionUserTokenInvalid);
            }

            var currentTime = _clock.GetCurrentTime();

            if (entity.ExpireAt.HasValue && entity.ExpireAt > currentTime)
            {
                throw new UserTokenExpiredException(token, entity.ExpireAt.Value, currentTime);
            }

            return new UserTokenInfo()
            {
                UserId = entity.UserId,
                ExpireAt = entity.ExpireAt,
                CreateAt = entity.CreateAt
            };
        }

        /// <inheritdoc/>
        public async Task<bool> RevokeTokenAsync(string token)
        {
            var entity = await _databaseContext.UserTokens.Where(t => t.Token == token && t.Deleted == false).SingleOrDefaultAsync();
            if (entity is not null)
            {
                entity.Deleted = true;
                await _databaseContext.SaveChangesAsync();

                _logger.LogInformation("A token is revoked with user id {}.", entity.UserId);

                return entity.ExpireAt <= _clock.GetCurrentTime();
            }
            return false;
        }

        /// <inheritdoc/>
        public async Task RevokeAllTokenByUserIdAsync(long userId)
        {
            List<UserTokenEntity> entities = await _databaseContext.UserTokens.Where(t => t.UserId == userId && t.Deleted == false).ToListAsync();
            foreach (var entity in entities)
            {
                entity.Deleted = true;
            }
            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation("All tokens of user with id {} are revoked.", userId);
        }
    }
}