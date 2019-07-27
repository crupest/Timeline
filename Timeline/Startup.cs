using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Timeline.Authenticate;
using Timeline.Configs;
using Timeline.Models;
using Timeline.Services;

namespace Timeline
{
    public class Startup
    {
        private const string corsPolicyName = "MyPolicy";

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public IHostingEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddCors(options =>
            {
                options.AddPolicy(corsPolicyName, builder =>
                {
                    if (Environment.IsProduction())
                        builder.WithOrigins("https://www.crupest.xyz", "https://crupest.xyz").AllowAnyMethod().AllowAnyHeader().AllowCredentials();
                    else
                        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            services.Configure<JwtConfig>(Configuration.GetSection(nameof(JwtConfig)));
            var jwtConfig = Configuration.GetSection(nameof(JwtConfig)).Get<JwtConfig>();

            services.AddAuthentication(AuthConstants.Scheme)
                .AddScheme<AuthOptions, AuthHandler>(AuthConstants.Scheme, AuthConstants.DisplayName, o => { });

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddTransient<IPasswordService, PasswordService>();

            var databaseConfig = Configuration.GetSection(nameof(DatabaseConfig)).Get<DatabaseConfig>();

            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseMySql(databaseConfig.ConnectionString)
                .ConfigureWarnings(warnings =>
                {
                    if (Environment.IsProduction())
                        warnings.Log(RelationalEventId.QueryClientEvaluationWarning);
                    else
                        warnings.Throw(RelationalEventId.QueryClientEvaluationWarning);
                });
            });

            services.AddHttpClient();

            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseCors(corsPolicyName);

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }
    }
}
