using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Timeline.Services.User.Avatar
{
    public static class UserAvatarServicesServiceCollectionExtensions
    {
        public static void AddUserAvatarServices(this IServiceCollection services)
        {
            services.TryAddScoped<IUserAvatarService, UserAvatarService>();
            services.TryAddScoped<IDefaultUserAvatarProvider, DefaultUserAvatarProvider>();
        }
    }
}
