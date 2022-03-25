using System.Collections.Generic;
using System.Threading.Tasks;

namespace Timeline.Services.User
{
    public static class UserServiceExtensions
    {
        public static async Task ThrowIfUserNotExist(this IUserService service, long userId)
        {
            if (!await service.CheckUserExistenceAsync(userId))
            {
                throw new EntityNotExistException(EntityTypes.User,
                    new Dictionary<string, object> { ["id"] = userId });
            }
        }
    }
}
