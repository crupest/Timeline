using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Timeline.Services.User.Avatar;
using Timeline.Services.User.RegisterCode;

namespace Timeline.Services.User
{
    public static class UserServicesServiceCollectionExtensions
    {
        public static IServiceCollection AddUserServices(this IServiceCollection services)
        {
            services.TryAddTransient<IPasswordService, PasswordService>();
            services.TryAddScoped<IUserService, UserService>();
            services.TryAddScoped<IUserDeleteService, UserDeleteService>();
            services.TryAddScoped<IUserPermissionService, UserPermissionService>();

            services.AddUserAvatarServices();

            services.TryAddScoped<IRegisterCodeService, RegisterCodeService>();

            return services;
        }
    }
}
