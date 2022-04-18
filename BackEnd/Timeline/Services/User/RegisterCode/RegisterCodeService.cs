using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Timeline.Entities;
using Timeline.Helpers;

namespace Timeline.Services.User.RegisterCode
{
    public class RegisterCodeService : IRegisterCodeService, IDisposable
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IUserService _userService;

        private readonly RandomNumberGenerator _randomNumberGenerator;

        public RegisterCodeService(DatabaseContext databaseContext, IUserService userService)
        {
            _databaseContext = databaseContext;
            _userService = userService;

            _randomNumberGenerator = RandomNumberGenerator.Create();
        }

        public async Task<string> CreateNewCodeAsync(long userId)
        {
            await _userService.CheckUserExistenceAsync(userId);

            var oldEntity = await _databaseContext.RegisterCodes.Where(r => r.OwnerId == userId && r.Enabled).SingleOrDefaultAsync();

            if (oldEntity is not null)
            {
                oldEntity.Enabled = false;
            }

            var newEntity = new Entities.RegisterCode()
            {
                Code = _randomNumberGenerator.GenerateAlphaDigitString(6),
                OwnerId = userId,
                Enabled = true
            };
            _databaseContext.RegisterCodes.Add(newEntity);

            await _databaseContext.SaveChangesAsync();

            return newEntity.Code;
        }

        public async Task<UserRegisterInfo> CreateRegisterInfoAsync(long userId, string registerCode, DateTime registerTime)
        {
            await _userService.CheckUserExistenceAsync(userId);

            var introducerId = await GetCodeOwnerAsync(registerCode, false);

            var entity = new UserRegisterInfo()
            {
                UserId = userId,
                IntroducerId = introducerId,
                RegisterCode = registerCode,
                RegisterTime = registerTime
            };

            _databaseContext.UserRegisterInfos.Add(entity);
            await _databaseContext.SaveChangesAsync();

            return entity;
        }

        public void Dispose()
        {
            _randomNumberGenerator.Dispose();
        }

        public async Task<long?> GetCodeOwnerAsync(string code, bool onlyEnabled = true)
        {
            var entity = await _databaseContext.RegisterCodes.Where(r => r.Code == code).SingleOrDefaultAsync();
            if (entity is null) return null;
            if (onlyEnabled && !entity.Enabled) return null;
            return entity.OwnerId;
        }

        public async Task<string?> GetCurrentCodeAsync(long userId)
        {
            await _userService.CheckUserExistenceAsync(userId);

            var entity = await _databaseContext.RegisterCodes.Where(r => r.OwnerId == userId && r.Enabled).SingleOrDefaultAsync();
            return entity?.Code;
        }

        public async Task<UserRegisterInfo?> GetUserRegisterInfoAsync(long userId)
        {
            await _userService.CheckUserExistenceAsync(userId);
            return await _databaseContext.UserRegisterInfos.Where(i => i.UserId == userId).SingleOrDefaultAsync();
        }

        public Task<List<UserRegisterInfo>> GetUserRegisterInfoOfIntroducerAsync(long introducerId)
        {
            throw new NotImplementedException();
        }
    }
}

