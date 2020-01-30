using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json.Serialization;
using Timeline.Auth;
using Timeline.Configs;
using Timeline.Entities;
using Timeline.Formatters;
using Timeline.Helpers;
using Timeline.Models.Converters;
using Timeline.Services;

namespace Timeline
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
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
            services.AddControllers(setup =>
            {
                setup.InputFormatters.Add(new StringInputFormatter());
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new JsonDateTimeConverter());
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = InvalidModelResponseFactory.Factory;
            });

            services.Configure<JwtConfig>(Configuration.GetSection(nameof(JwtConfig)));
            services.AddAuthentication(AuthenticationConstants.Scheme)
                .AddScheme<MyAuthenticationOptions, MyAuthenticationHandler>(AuthenticationConstants.Scheme, AuthenticationConstants.DisplayName, o => { });
            services.AddAuthorization();


            if (Environment.IsDevelopment())
            {
                services.AddCors(setup =>
                {
                    setup.AddDefaultPolicy(builder =>
                    {
                        builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
                    });
                });
            }
            else
            {
                var corsConfig = Configuration.GetSection("Cors").Get<string[]>();
                services.AddCors(setup =>
                {
                    setup.AddDefaultPolicy(builder =>
                    {
                        builder.AllowAnyHeader().AllowAnyMethod().WithOrigins(corsConfig);
                    });
                });
            }

            services.AddAutoMapper(GetType().Assembly);

            services.AddTransient<IClock, Clock>();

            services.AddTransient<IPasswordService, PasswordService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserTokenService, JwtUserTokenService>();
            services.AddScoped<IUserTokenManager, UserTokenManager>();
            services.AddUserAvatarService();

            services.AddScoped<IPersonalTimelineService, PersonalTimelineService>();

            var databaseConfig = Configuration.GetSection(nameof(DatabaseConfig)).Get<DatabaseConfig>();

            if (databaseConfig.UseDevelopment)
            {
                services.AddDbContext<DatabaseContext, DevelopmentDatabaseContext>(options =>
                {
                    if (databaseConfig.DevelopmentConnectionString == null)
                        throw new InvalidOperationException("DatabaseConfig.DevelopmentConnectionString is not set. Please set it as a sqlite connection string.");
                    options.UseSqlite(databaseConfig.DevelopmentConnectionString);
                });
            }
            else
            {
                services.AddDbContext<DatabaseContext, ProductionDatabaseContext>(options =>
                {
                    if (databaseConfig.ConnectionString == null)
                        throw new InvalidOperationException("DatabaseConfig.ConnectionString is not set. Please set it as a mysql connection string.");
                    options.UseMySql(databaseConfig.ConnectionString);
                });
            }
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
