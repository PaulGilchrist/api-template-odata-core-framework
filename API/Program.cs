using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ODataCoreTemplate
{
    public class Program {
        public static async Task Main(string[] args) {
            IWebHost webHost = CreateWebHostBuilder(args).Build();
            using (var scope = webHost.Services.CreateScope()) {
                // Get the ClientPolicyStore instance
                var clientPolicyStore = scope.ServiceProvider.GetRequiredService<IClientPolicyStore>();
                // Seed client data from appsettings
                await clientPolicyStore.SeedAsync();
            }
            await webHost.RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseApplicationInsights()
                .UseStartup<Startup>().UseKestrel(options => {
                    options.Limits.MaxRequestLineSize = 65536;
                });
    }
}
