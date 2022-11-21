using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;
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
using Timeline.SignalRHub;
using Timeline.Swagger;

namespace Timeline
{
    public class Startup
    {
        private readonly bool _enableForwardedHeaders;
        private readonly string? _forwardedHeadersAllowedProxyHostsString;
        private readonly List<string>? _forwardedHeadersAllowedProxyHosts = null;
        private readonly List<List<IPAddress>>? _forwardedHeadersAllowedProxyIPs = null;
        private readonly FrontEndMode _frontEndMode;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("We are in environment: " + environment.EnvironmentName);
            Console.ResetColor();

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
                    Console.WriteLine("Unknown FrontEnd configuration value '{0}', fallback to normal.", frontEndModeString);
                }
            }

            _enableForwardedHeaders = ApplicationConfiguration.GetBoolConfig(configuration, ApplicationConfiguration.EnableForwardedHeadersKey, false);
            _forwardedHeadersAllowedProxyHostsString = Configuration.GetValue<string?>(ApplicationConfiguration.ForwardedHeadersAllowedProxyHostsKey);

            if (_enableForwardedHeaders)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Forwarded headers enabled.");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Yellow;
                if (_forwardedHeadersAllowedProxyHostsString is not null)
                {
                    _forwardedHeadersAllowedProxyHosts = new List<string>();
                    foreach (var host in _forwardedHeadersAllowedProxyHostsString.Split(new char[] { ';', ',' }))
                    {
                        _forwardedHeadersAllowedProxyHosts.Add(host.Trim());
                    }

                    _forwardedHeadersAllowedProxyIPs = new();
                    foreach (var host in _forwardedHeadersAllowedProxyHosts)
                    {
                        // Resolve host to ip
                        var ips = System.Net.Dns.GetHostAddresses(host);
                        _forwardedHeadersAllowedProxyIPs.Add(new(ips));
                    }

                    Console.WriteLine("Allowed proxy hosts:");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    StringBuilder log = new();
                    for (int i = 0; i < _forwardedHeadersAllowedProxyHosts.Count; i++)
                    {
                        log.Append(_forwardedHeadersAllowedProxyHosts[i]);
                        log.Append(" (");
                        log.Append(string.Join(' ', _forwardedHeadersAllowedProxyIPs[i]));
                        log.Append(")\n");
                    }
                    Console.WriteLine(log.ToString());
                }
                else
                {
                    Console.WriteLine("Allowed proxy hosts settings is default");
                }
                Console.ResetColor();
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
                setup.Filters.Add<CatchEntityDeletedExceptionFilter>();
                setup.Filters.Add<CatchImageExceptionFilter>();
                setup.UseApiRoutePrefix("api");
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new JsonDateTimeConverter());
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = InvalidModelResponseFactory.Factory;
            });

            services.AddSignalR();

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

            services.AddScoped<ITimelineBookmarkService1, TimelineBookmarkService1>();

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

            if (_enableForwardedHeaders)
            {
                services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
                    if (_forwardedHeadersAllowedProxyHostsString is not null)
                    {
                        options.KnownNetworks.Clear();
                        options.KnownProxies.Clear();
                        foreach (var ips in _forwardedHeadersAllowedProxyIPs!)
                        {
                            ips.ForEach(ip => options.KnownProxies.Add(ip));
                        }
                    }
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

            if (_enableForwardedHeaders)
            {
                app.UseForwardedHeaders();
            }

            app.UseOpenApi();
            app.UseReDoc();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<TimelineHub>("api/hub/timeline");
            });

            UnknownEndpointMiddleware.Attach(app);

            if (_frontEndMode != FrontEndMode.Disable)
            {
                app.UseSpa(spa =>
                {
                });
            }
        }
    }
}
