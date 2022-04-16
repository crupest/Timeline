using System.Collections.Generic;

namespace Timeline.Services.User.RegisterCode
{
    public interface IRegisterCodeService
    {
        string? GetCurrentRegisterCode(long userId);

        List<string> GetAllRegisterCodes(long userId);

        string CreateNewRegisterCode(long userId);

        List<long> GetUsersIntroducedByCode(string registerCode);

        List<long> GetUsersIntroducedByUser(long userId);
    }
}
