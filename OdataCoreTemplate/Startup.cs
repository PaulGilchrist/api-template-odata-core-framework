using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using NJsonSchema;
using NSwag.AspNetCore;
using OdataCoreTemplate.Models;

namespace OdataCoreTemplate
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
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
            // needed to load configuration from appsettings.json
            services.AddOptions();
            // needed to store rate limit counters and ip rules
            services.AddMemoryCache();
            //load general configuration from appsettings.json
            services.Configure<ClientRateLimitOptions>(Configuration.GetSection("ClientRateLimiting"));
            //load client rules from appsettings.json
            services.Configure<ClientRateLimitPolicies>(Configuration.GetSection("ClientRateLimitPolicies"));
            //load general configuration from appsettings.json
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            //load ip rules from appsettings.json
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));
            // inject counter and rules stores
            services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddMvc()
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
            }
  			app.UseSwaggerUi(typeof(Startup).GetTypeInfo().Assembly, settings => {
	  		    settings.GeneratorSettings.Title="ODataCoreTemplate";
				settings.GeneratorSettings.DefaultPropertyNameHandling = PropertyNameHandling.CamelCase;
				settings.GeneratorSettings.Description="OData .Net Core OpenAPI Project Template";
				settings.GeneratorSettings.IsAspNetCore = true;
				// settings.GeneratorSettings.DefaultUrlTemplate.
				// settings.GeneratorSettings.SerializerSettings.
			});
            app.UseAuthentication();
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseClientRateLimiting();
            app.UseIpRateLimiting();
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
