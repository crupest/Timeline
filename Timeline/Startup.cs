using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json.Serialization;
using TimelineApp.Auth;
using TimelineApp.Configs;
using TimelineApp.Entities;
using TimelineApp.Formatters;
using TimelineApp.Helpers;
using TimelineApp.Models.Converters;
using TimelineApp.Services;

namespace TimelineApp
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
            var workDir = Configuration.GetValue<string?>("WorkDir");
            if (workDir == null)
            {
                throw new InvalidOperationException("Please set a work directory first.");
            }

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

            services.Configure<JwtConfiguration>(Configuration.GetSection("Jwt"));
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

            services.AddScoped<IPathProvider, PathProvider>();

            services.AddAutoMapper(GetType().Assembly);

            services.AddTransient<IClock, Clock>();

            services.AddTransient<IPasswordService, PasswordService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserTokenService, JwtUserTokenService>();
            services.AddScoped<IUserTokenManager, UserTokenManager>();

            services.AddScoped<IETagGenerator, ETagGenerator>();
            services.AddScoped<IDataManager, DataManager>();

            services.AddScoped<IImageValidator, ImageValidator>();

            services.AddUserAvatarService();

            services.AddScoped<ITimelineService, TimelineManager>();
            services.AddScoped<IPersonalTimelineService, PersonalTimelineService>();

            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddDbContext<DatabaseContext>((services, options )=>
            {
                var pathProvider = services.GetRequiredService<IPathProvider>();
                options.UseSqlite($"Data Source={pathProvider.GetDatabaseFilePath()}");
            });
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
