using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using OdataCoreTemplate.Classes;
using OdataCoreTemplate.Models;
using ODataCoreTemplate.Models;
using Swashbuckle.AspNetCore.Swagger;
using System.Linq;

namespace ODataCoreTemplate {
    public class Startup
    {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddDbContext<ApiContext>(opt => opt.UseInMemoryDatabase("ApiDb"));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options => {
                   options.Authority = "https://login.microsoftonline.com/" + Configuration.GetValue<string>("Security:TenantIdentifier");
                   options.TokenValidationParameters = new TokenValidationParameters {
                       ValidAudiences = Configuration.GetValue<string>("Security:AllowedAudiences").Split(',')
                   };
               });
            services.AddOData();
            // Workaround to support OData and Swashbuckle working together: https://github.com/OData/WebApi/issues/1177
            services.AddMvcCore(options => {
                foreach (var outputFormatter in options.OutputFormatters.OfType<ODataOutputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0)) {
                    outputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
                foreach (var inputFormatter in options.InputFormatters.OfType<ODataInputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0)) {
                    inputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
            }).AddApiExplorer();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options => {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.ContractResolver =
                    new Newtonsoft.Json.Serialization.DefaultContractResolver();
                });
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new Info {
                    Title = "OData Core Template",
                    Description = "A simple example ASP.NET Core Web API leveraging OData, OAuth, and Swagger/Open API",
                    Version = "v1"
                });
                // Workaround to show OData input parameters in Swashbuckle (waiting on Swashbuckle.AspNetCore.Odata NuGet package)
                c.OperationFilter<SwaggerODataOperationFilter>();
            });
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
            app.UseAuthentication();
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OData Core Template API v1");
                //c.RoutePrefix = string.Empty;
            });
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
