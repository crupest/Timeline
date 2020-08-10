using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json.Serialization;
using Timeline.Auth;
using Timeline.Configs;
using Timeline.Entities;
using Timeline.Formatters;
using Timeline.Helpers;
using Timeline.Models.Converters;
using Timeline.Routes;
using Timeline.Services;

namespace Timeline
{
    public class Startup
    {
        private readonly bool disableFrontEnd;
        private readonly bool useMockFrontEnd;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Environment = environment;
            Configuration = configuration;

            disableFrontEnd = Configuration.GetValue<bool?>(ApplicationConfiguration.DisableFrontEndKey) ?? false;
            useMockFrontEnd = Configuration.GetValue<bool?>(ApplicationConfiguration.UseMockFrontEndKey) ?? false;
        }

        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(setup =>
            {
                setup.InputFormatters.Add(new StringInputFormatter());
                setup.UseApiRoutePrefix("api");
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new JsonDateTimeConverter());
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = InvalidModelResponseFactory.Factory;
            });

            services.Configure<JwtConfiguration>(Configuration.GetSection("Jwt"));
            services.AddAuthentication(AuthenticationConstants.Scheme)
                .AddScheme<MyAuthenticationOptions, MyAuthenticationHandler>(AuthenticationConstants.Scheme, AuthenticationConstants.DisplayName, o => { });
            services.AddAuthorization();

            services.AddSingleton<IPathProvider, PathProvider>();

            services.AddSingleton<IDatabaseBackupService, DatabaseBackupService>();

            services.AddAutoMapper(GetType().Assembly);

            services.AddTransient<IClock, Clock>();

            services.AddTransient<IPasswordService, PasswordService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserDeleteService, UserDeleteService>();
            services.AddScoped<IUserTokenService, JwtUserTokenService>();
            services.AddScoped<IUserTokenManager, UserTokenManager>();

            services.AddScoped<IETagGenerator, ETagGenerator>();
            services.AddScoped<IDataManager, DataManager>();

            services.AddScoped<IImageValidator, ImageValidator>();

            services.AddUserAvatarService();

            services.AddScoped<ITimelineService, TimelineService>();

            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddDbContext<DatabaseContext>((services, options) =>
            {
                var pathProvider = services.GetRequiredService<IPathProvider>();
                options.UseSqlite($"Data Source={pathProvider.GetDatabaseFilePath()}");
            });

            if (!disableFrontEnd)
            {
                if (useMockFrontEnd)
                {
                    services.AddSpaStaticFiles(config =>
                    {
                        config.RootPath = "MockClientApp";
                    });

                }
                else if (!Environment.IsDevelopment()) // In development, we don't want to serve dist. Or it will take precedence than front end dev server.
                {
                    services.AddSpaStaticFiles(config =>
                    {
                        config.RootPath = "ClientApp/dist";
                    });
                }
            }
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            if (!disableFrontEnd && (useMockFrontEnd || !Environment.IsDevelopment()))
            {
                app.UseSpaStaticFiles(new StaticFileOptions
                {
                    ServeUnknownFileTypes = true
                });
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            UnknownEndpointMiddleware.Attach(app);

            if (!disableFrontEnd)
            {
                app.UseSpa(spa =>
                {
                    spa.Options.SourcePath = useMockFrontEnd ? "MockClientApp" : "ClientApp";

                    if (!useMockFrontEnd && (Configuration.GetValue<bool?>(ApplicationConfiguration.UseProxyFrontEndKey) ?? false))
                    {
                        spa.UseProxyToSpaDevelopmentServer(new UriBuilder("http", "localhost", 3000).Uri);
                    }
                });
            }
        }
    }
}
