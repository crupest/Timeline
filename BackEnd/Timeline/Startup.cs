using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Timeline.Auth;
using Timeline.Configs;
using Timeline.Entities;
using Timeline.Filters;
using Timeline.Formatters;
using Timeline.Helpers;
using Timeline.Models;
using Timeline.Models.Converters;
using Timeline.Routes;
using Timeline.Services;
using Timeline.Services.Api;
using Timeline.Services.Data;
using Timeline.Services.DatabaseManagement;
using Timeline.Services.Imaging;
using Timeline.Services.Mapper;
using Timeline.Services.Timeline;
using Timeline.Services.Token;
using Timeline.Services.User;
using Timeline.Swagger;

namespace Timeline
{
    public class Startup
    {
        private readonly FrontEndMode _frontEndMode;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Environment = environment;
            Configuration = configuration;

            var frontEndModeString = Configuration.GetValue<string?>(ApplicationConfiguration.FrontEndKey);

            if (frontEndModeString is null)
            {
                _frontEndMode = FrontEndMode.Normal;
            }
            else
            {
                if (!Enum.TryParse(frontEndModeString, true, out _frontEndMode))
                {
                    _frontEndMode = FrontEndMode.Normal;
                    Console.WriteLine("Unknown FrontEnd configuaration value '{0}', fallback to normal.", frontEndModeString);
                }
            }
        }

        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            TypeDescriptor.AddAttributes(typeof(DateTime), new TypeConverterAttribute(typeof(MyDateTimeConverter)));

            services.AddControllers(setup =>
            {
                setup.InputFormatters.Add(new StringInputFormatter());
                setup.InputFormatters.Add(new ByteDataInputFormatter());
                setup.Filters.Add(new ConsumesAttribute(MimeTypes.ApplicationJson, MimeTypes.TextJson));
                setup.Filters.Add(new ProducesAttribute(MimeTypes.ApplicationJson, MimeTypes.TextJson));
                setup.Filters.Add<CatchEntityNotExistExceptionFilter>();
                setup.Filters.Add<CatchEntityAlreadyExistExceptionFilter>();
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

            services.AddAuthentication(AuthenticationConstants.Scheme)
                .AddScheme<MyAuthenticationOptions, MyAuthenticationHandler>(AuthenticationConstants.Scheme, AuthenticationConstants.DisplayName, o => { });
            services.AddAuthorization();
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();


            services.AddDbContext<DatabaseContext>((services, options) =>
            {
                var pathProvider = services.GetRequiredService<IPathProvider>();
                options.UseSqlite($"Data Source={pathProvider.GetDatabaseFilePath()}");
            });

            services.AddBasicServices();
            services.AddDatabaseManagementService();
            services.AddDataServices();
            services.AddImageServices();
            services.AddUserServices();
            services.AddTokenServices(Configuration);

            services.AddTimelineServices();

            services.AddMappers();

            services.AddScoped<IHighlightTimelineService, HighlightTimelineService>();
            services.AddScoped<IBookmarkTimelineService, BookmarkTimelineService>();
            services.AddScoped<ISearchService, SearchService>();

            services.AddOpenApiDocs();

            if (_frontEndMode == FrontEndMode.Mock)
            {
                services.AddSpaStaticFiles(config =>
                {
                    config.RootPath = "MockClientApp";
                });

            }
            else if (_frontEndMode == FrontEndMode.Normal)
            {
                services.AddSpaStaticFiles(config =>
                {
                    config.RootPath = "ClientApp";
                });
            }
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            if (_frontEndMode == FrontEndMode.Mock || _frontEndMode == FrontEndMode.Normal)
            {
                app.UseSpaStaticFiles(new StaticFileOptions
                {
                    ServeUnknownFileTypes = true
                });
            }

            app.UseOpenApi();
            app.UseReDoc();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            UnknownEndpointMiddleware.Attach(app);

            if (_frontEndMode != FrontEndMode.Disable)
            {
                app.UseSpa(spa =>
                {
                    if (_frontEndMode == FrontEndMode.Proxy)
                    {
                        spa.UseProxyToSpaDevelopmentServer(new UriBuilder("http", "localhost", 3000).Uri);
                    }
                });
            }
        }
    }
}
