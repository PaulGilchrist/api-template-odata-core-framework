using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using OdataCoreTemplate.Models;
using ODataCoreTemplate.Models;

namespace ODataCoreTemplate
{
    public class Startup
    {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddDbContext<ApiContext>(opt => opt.UseInMemoryDatabase("ApiDb"));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
	            .AddJsonOptions(options => {
                     options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                     options.SerializerSettings.ContractResolver =
                     new Newtonsoft.Json.Serialization.DefaultContractResolver();
                 });
            services.AddOData();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseHsts();
            }
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseHttpsRedirection();
            app.UseMvc(b => {
                b.Select().Expand().Filter().OrderBy().MaxTop(100).Count();
                b.MapODataServiceRoute("ODataRoute", "odata", GetEdmModel());
                b.EnableDependencyInjection();
            });
        }

        private static IEdmModel GetEdmModel() {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.Namespace = "ApiTemplate";
            builder.ContainerName = "ApiTemplateContainer";
            builder.EnableLowerCamelCase();
            builder.EntitySet<User>("Users");
            return builder.GetEdmModel();
        }

    }
}
