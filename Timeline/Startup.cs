using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Timeline.Configs;
using Timeline.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Timeline.Formatters;

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
            services.AddMvc(options =>
            {
                options.InputFormatters.Add(new StringInputFormatter());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddCors(options =>
            {
                if (Environment.IsProduction())
                {
                    options.AddPolicy(corsPolicyName, builder =>
                    {
                        builder.WithOrigins("www.crupest.xyz", "crupest.xyz").AllowAnyMethod().AllowAnyHeader();
                    });
                }
                else
                {
                    options.AddPolicy(corsPolicyName, builder =>
                    {
                        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    });
                }
            });

            services.Configure<JwtConfig>(Configuration.GetSection(nameof(JwtConfig)));
            var jwtConfig = Configuration.GetSection(nameof(JwtConfig)).Get<JwtConfig>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters.ValidateIssuer = true;
                    o.TokenValidationParameters.ValidateAudience = true;
                    o.TokenValidationParameters.ValidateIssuerSigningKey = true;
                    o.TokenValidationParameters.ValidateLifetime = true;
                    o.TokenValidationParameters.ValidIssuer = jwtConfig.Issuer;
                    o.TokenValidationParameters.ValidAudience = jwtConfig.Audience;
                    o.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfig.SigningKey));
                });

            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IJwtService, JwtService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseCors(corsPolicyName);

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
