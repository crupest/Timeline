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
        private readonly RandomNumberGenerator _randomNumberGenerator;

        public RegisterCodeService(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
            _randomNumberGenerator = RandomNumberGenerator.Create();
        }

        public async Task<string> CreateNewCode(long userId)
        {
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

        public Task<UserRegisterInfo> CreateRegisterInfo(long userId, long introducerId, string registerCode, DateTime registerTime)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _randomNumberGenerator.Dispose();
        }

        public async Task<long?> GetCodeOwner(string code, bool onlyEnabled = true)
        {
            var entity = await _databaseContext.RegisterCodes.Where(r => r.Code == code).SingleOrDefaultAsync();
            if (entity is null) return null;
            if (onlyEnabled && !entity.Enabled) return null;
            return entity.OwnerId;
        }

        public async Task<string?> GetCurrentCode(long userId)
        {
            var entity = await _databaseContext.RegisterCodes.Where(r => r.OwnerId == userId && r.Enabled).SingleOrDefaultAsync();
            return entity?.Code;
        }

        public Task<UserRegisterInfo?> GetUserRegisterInfo(long userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserRegisterInfo>> GetUserRegisterInfoOfIntroducer(long introducerId)
        {
            throw new NotImplementedException();
        }
    }
}

