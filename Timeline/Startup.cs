using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Timeline.Authenticate;
using Timeline.Configs;
using Timeline.Entities;
using Timeline.Helpers;
using Timeline.Services;

namespace Timeline
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = InvalidModelResponseFactory.Factory;
                })
                .AddNewtonsoftJson();

            services.Configure<JwtConfig>(Configuration.GetSection(nameof(JwtConfig)));
            var jwtConfig = Configuration.GetSection(nameof(JwtConfig)).Get<JwtConfig>();
            services.AddAuthentication(AuthConstants.Scheme)
                .AddScheme<AuthOptions, AuthHandler>(AuthConstants.Scheme, AuthConstants.DisplayName, o => { });

            var corsConfig = Configuration.GetSection("Cors").Get<string[]>();
            services.AddCors(setup =>
            {
                setup.AddDefaultPolicy(new CorsPolicyBuilder()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins(corsConfig).Build()
                );
            });

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddTransient<IPasswordService, PasswordService>();
            services.AddTransient<IClock, Clock>();

            services.AddUserAvatarService();
            services.AddUserDetailService();

            var databaseConfig = Configuration.GetSection(nameof(DatabaseConfig)).Get<DatabaseConfig>();

            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseMySql(databaseConfig.ConnectionString);
            });

            services.AddHttpClient();

            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
