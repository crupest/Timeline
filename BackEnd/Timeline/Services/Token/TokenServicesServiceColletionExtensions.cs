using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Timeline.Configs;

namespace Timeline.Services.Token
{
    public static class TokenServicesServiceColletionExtensions
    {
        public static IServiceCollection AddTokenServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TokenOptions>(configuration.GetSection("Token"));
            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
            services.AddScoped<IUserTokenHandler, JwtUserTokenHandler>();
            services.AddScoped<IUserTokenManager, UserTokenManager>();
            return services;
        }
    }
}
