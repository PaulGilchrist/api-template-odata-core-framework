using API.Classes;
using API.Configuration;
using API.Data;
using AspNetCoreRateLimit;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ODataCoreTemplate {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            // To make this demo simpler, we can use a memory only database populated with mock data
            services.AddDbContext<ApiDbContext>(opt => opt.UseInMemoryDatabase("ApiDb"), Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton);

            /* For this demo we are using an in-memory database, but later we will connect to an actual database
            *   var connectionString = Configuration.GetValue<string>("Sql:ConnectionString");
            *   var maxRetryCount = Configuration.GetValue<int>("Sql:MaxRetryCount");
            *   var maxRetryDelay = Configuration.GetValue<int>("Sql:MaxRetryDelay");
            *   var maxPoolSize = Configuration.GetValue<int>("Sql:MaxPoolSize");
            *   services.AddDbContextPool<ApiDbContext>(options => options.UseSqlServer(connectionString, o => o.EnableRetryOnFailure(maxRetryCount, TimeSpan.FromSeconds(maxRetryDelay), null)), maxPoolSize);
            */

            // CORS support
            services.AddCors(options => {
                options.AddPolicy("AllOrigins",
                     builder => {
                         builder
                     .AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader();
                     });
            });
            // Add Security
            services.AddSingleton<Security>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               // Configure OAuth Authentication
               .AddJwtBearer(options => {
                   options.Authority = "https://login.microsoftonline.com/" + Configuration.GetValue<string>("Security:TenantIdentifier");
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidAudiences = Configuration.GetValue<string>("Security:AllowedAudiences").Split(',')
                   };
               })
                // Configure Basic Authentication
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

            // Needed to load configuration from appsettings.json
            services.AddOptions();
            // The following line enables Application Insights telemetry collection.
            services.AddApplicationInsightsTelemetry();
            services.AddSingleton<TelemetryTracker>();
            // Needed to store rate limit counters and ip rules
            services.AddMemoryCache(); services.AddMemoryCache();
            // Load general configuration from appsettings.json
            services.Configure<ClientRateLimitOptions>(Configuration.GetSection("ClientRateLimiting"));
            // Load client rules from appsettings.json
            services.Configure<ClientRateLimitPolicies>(Configuration.GetSection("ClientRateLimitPolicies"));
            // Inject counter and rules stores
            services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddMvc(options => options.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddNewtonsoftJson(options => {
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    options.SerializerSettings.Formatting = Formatting.None;
                    options.SerializerSettings.PreserveReferencesHandling=PreserveReferencesHandling.None;
                    options.SerializerSettings.NullValueHandling=NullValueHandling.Ignore;
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });
            services.AddApiVersioning(options => {
                //options.ApiVersionReader = new QueryStringApiVersionReader();
                options.ReportApiVersions = true;
                // required when adding versioning to and existing API to allow existing non-versioned queries to succeed (not error with no version specified)
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = ApiVersions.V2;
                // Needed to fix issue #1754 - https://github.com/OData/WebApi/issues/1754
                options.RegisterMiddleware = false;
           });
            services.AddOData().EnableApiVersioning();
            services.AddODataApiExplorer(options => {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";
            });
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(options => {
                options.IncludeXmlComments(XmlCommentsFilePath);
                //Configure Swagger to filter out $expand objects to improve performance for large highly relational APIs
                options.SchemaFilter<SwaggerIgnoreFilter>();
                //options.DescribeAllEnumsAsStrings();
                options.OperationFilter<SwaggerDefaultValues>();
                // The following two options are only needed is using "Basic" security
                options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme() { In = ParameterLocation.Header, Description = "Please insert Basic token into field", Name = "Authorization", Type = SecuritySchemeType.ApiKey });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Basic"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            // The IHttpContextAccessor service is not registered by default.  The clientId/clientIp resolvers use it.  https://github.com/aspnet/Hosting/issues/793
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            // configuration (resolvers, counter key builders)
            services.AddSingleton<IRateLimitConfiguration, CustomRateLimitConfiguration>();
            // Add support for GetUrlHelper used in ReferenceHelper class
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(x => {
                var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
                var factory = x.GetRequiredService<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            });
            services.AddSwaggerGenNewtonsoftSupport();
        }

        /// <summary>
        /// Configures the application using the provided builder, hosting environment, and logging factory.
        /// </summary>
        /// <param name="app">The current application builder.</param>
        /// <param name="env">The current hosting environment.</param>
        /// <param name="httpContextAccessor">Allows access to the HTTP context including request/response</param>
        /// <param name="modelBuilder">The <see cref="VersionedODataModelBuilder">model builder</see> used to create OData entity data models (EDMs).</param>
        /// <param name="provider">The API version descriptor provider used to enumerate defined API versions.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor, VersionedODataModelBuilder modelBuilder, IApiVersionDescriptionProvider provider) {
            if (env.EnvironmentName == "Development") {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseHsts();
            }
            var httpRequestLoggingLevel = Configuration.GetValue<string>("ApplicationInsights:HttpRequestLoggingLevel");
            app.UseCors("AllOrigins");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseClientRateLimiting();
            // Add mock data to the database if it is empty (demo uses in memory database only, so always starts empty)
            var context = app.ApplicationServices.GetService<ApiDbContext>();
            MockData.AddMockData(context);
            // Add custom telemetry initializer to add user name from the HTTP context
            var configuration = app.ApplicationServices.GetService<TelemetryConfiguration>();
            configuration.TelemetryInitializers.Add(new TelemetryInitializer(httpContextAccessor));
            app.UseMiddleware<CaptureRequestMiddleware>(httpRequestLoggingLevel);
            app.UseODataBatching();
            app.UseApiVersioning(); // added to fix issue outlined in https://github.com/OData/WebApi/issues/1754
            app.UseMvc(routes => {
                routes.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
                routes.MapVersionedODataRoutes("ODataRoute", "odata", modelBuilder.GetEdmModels());
                routes.MapODataServiceRoute("ODataBatch", null,
                   configureAction: containerBuilder => containerBuilder
                       .AddService(Microsoft.OData.ServiceLifetime.Singleton, typeof(IEdmModel),
                           sp => modelBuilder.GetEdmModels().First())
                       .AddService(Microsoft.OData.ServiceLifetime.Singleton, typeof(IEnumerable<IODataRoutingConvention>),
                           sp => ODataRoutingConventions.CreateDefaultWithAttributeRouting("ODataBatch", routes))
                       .AddService(Microsoft.OData.ServiceLifetime.Singleton, typeof(ODataBatchHandler),
                           sp => {
                               var oDataBatchHandler = new TransactionalODataBatchHandler();
                               oDataBatchHandler.MessageQuotas.MaxOperationsPerChangeset = 5000;
                               return oDataBatchHandler;
                           })
                        .AddService(Microsoft.OData.ServiceLifetime.Singleton, typeof(ODataMessageReaderSettings),
                            sp => {
                                ODataMessageReaderSettings odataMessageReaderSettings = new ODataMessageReaderSettings();
                                odataMessageReaderSettings.MessageQuotas.MaxOperationsPerChangeset = 5000;
                                return odataMessageReaderSettings;
                            })
                );
                routes.EnableDependencyInjection();
            });
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(options => {
                foreach (var description in provider.ApiVersionDescriptions) {
                    options.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                }
                options.DefaultModelExpandDepth(2);
                options.DefaultModelsExpandDepth(-1);
                options.DefaultModelRendering(ModelRendering.Model);
                options.DisplayRequestDuration();
                options.DocExpansion(DocExpansion.None);
            });
        }

        static string XmlCommentsFilePath {
            get {
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var fileName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
                return Path.Combine(basePath, fileName);
            }
        }


    }
}
