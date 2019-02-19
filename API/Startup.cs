using API.Classes;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using OdataCoreTemplate.Models;
using ODataCoreTemplate.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ODataCoreTemplate {
    public class Startup
    {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            // To make this demo simpler, we can use a memory only database populated with mock data
            services.AddDbContext<ApiDbContext>(opt => opt.UseInMemoryDatabase("ApiDb"), ServiceLifetime.Singleton);

            // For this demo we are using an in-memory database, but later we will connect to an actual database
            // https://docs.microsoft.com/en-us/ef/core/get-started/aspnetcore/new-db
            //var connection = @"data source=localhost;initial catalog=ApiDev;integrated security=True;MultipleActiveResultSets=True;ConnectRetryCount=3";
            //services.AddDbContext<ApiContext>(options => options.UseSqlServer(connection));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options => {
                   options.Authority = "https://login.microsoftonline.com/" + Configuration.GetValue<string>("Security:TenantIdentifier");
                   options.TokenValidationParameters = new TokenValidationParameters {
                       ValidAudiences = Configuration.GetValue<string>("Security:AllowedAudiences").Split(',')
                   };
               });
            services.AddOData();
            services.AddMvc(options => {
                    options.EnableEndpointRouting = false;
                // Workaround to support OData and Swashbuckle working together: https://github.com/OData/WebApi/issues/1177
                foreach (var outputFormatter in options.OutputFormatters.OfType<ODataOutputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0)) {
                    outputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
                foreach (var inputFormatter in options.InputFormatters.OfType<ODataInputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0)) {
                    inputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
            })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options => {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });
            services.AddHttpContextAccessor();
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new Info {
                    Title = "OData Core Template",
                    Description = "A simple example ASP.NET Core Web API leveraging OData, OAuth, and Swagger/Open API",
                    Version = "v1"
                });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseHsts();
            }
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            //Add mock data to the database if it is empty (demo uses in memory database only, so always starts empty)
            var context = app.ApplicationServices.GetService<ApiDbContext>();
            OdataCoreTemplate.Data.MockData.AddMockData(context);
            //Add custom telemetry initializer to add user name from the HTTP context
            var configuration = app.ApplicationServices.GetService<TelemetryConfiguration>();
            configuration.TelemetryInitializers.Add(new TelemetryInitializer(httpContextAccessor));
            app.UseMvc(b => {
                b.Select().Expand().Filter().OrderBy().MaxTop(100).Count();
                // Swagger will not find controllers using conventional routing.  Attribute routing is required.
                // Also, OData controller base class opts out of the API Explorer
                b.MapODataServiceRoute("ODataRoute", "odata", GetEdmModel(app.ApplicationServices));
                b.EnableDependencyInjection();
            });
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OData Core Template API v1");
                c.DocExpansion(DocExpansion.None);
            });
        }

        private static IEdmModel GetEdmModel(IServiceProvider applicationServices) {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.Namespace = "ApiTemplate";
            builder.ContainerName = "ApiTemplateContainer";
            builder.EnableLowerCamelCase();
            builder.EntitySet<User>("users")
                .EntityType
                .Filter()
                .Count()
                .Expand()
                .OrderBy()
                .Page()
                .Select()
                .HasMany(u => u.Addresses)
                .Expand();
            builder.EntitySet<Address>("addresses")
                .EntityType
                .Filter()
                .Count()
                .Expand()
                .OrderBy()
                .Page()
                .Select()
                .HasMany(a => a.Users)
                .Expand();
            return builder.GetEdmModel();
        }

    }
}
