using System.Collections.Generic;
using System.Threading.Tasks;

namespace Timeline.Services.User
{
    public static class BasicUserServiceExtensions
    {
        public static async Task ThrowIfUserNotExist(this IBasicUserService service, long userId)
        {
            if (!await service.CheckUserExistenceAsync(userId))
            {
                throw new EntityNotExistException(EntityTypes.User,
                    new Dictionary<string, object> { ["id"] = userId });
            }
        }
    }
}
