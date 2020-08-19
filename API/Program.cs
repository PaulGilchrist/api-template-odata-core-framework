using AspNetCoreRateLimit;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace ODataCoreTemplate {
    public static class Program {
        public static async Task Main(string[] args) {
            var webHost = CreateWebHostBuilder(args).Build();
            using (var scope = webHost.Services.CreateScope()) {
                // Get the ClientPolicyStore instance
                var clientPolicyStore = scope.ServiceProvider.GetRequiredService<IClientPolicyStore>();
                // Seed client data from appsettings
                await clientPolicyStore.SeedAsync();
            }
            await webHost.RunAsync();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => {
                webBuilder.ConfigureKestrel(serverOptions => {
                    // Set properties and call methods on options
                    serverOptions.Limits.MaxRequestLineSize = 65536;
                })
                .UseStartup<Startup>();
            });

        //public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //        .UseApplicationInsights()
        //        .UseStartup<Startup>().UseKestrel(options => {
        //            options.Limits.MaxRequestLineSize = 65536;
        //        });
    }
}
