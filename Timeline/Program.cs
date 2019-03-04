using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Timeline
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args)
            .ConfigureAppConfiguration((context, config) => 
            {
                if (context.HostingEnvironment.IsProduction())
                    config.AddJsonFile(new PhysicalFileProvider("/etc/webapp/timeline/"), "config.json", true, true);
            })
            .Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
