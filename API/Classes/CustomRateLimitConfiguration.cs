
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Linq;

namespace API.Classes {
    public class CustomRateLimitConfiguration : RateLimitConfiguration {

        public CustomRateLimitConfiguration(IHttpContextAccessor httpContextAccessor, IOptions<IpRateLimitOptions> ipOptions, IOptions<ClientRateLimitOptions> clientOptions) : base(httpContextAccessor, ipOptions, clientOptions) {
        }

        protected override void RegisterResolvers() {
            //base.RegisterResolvers();
            ClientResolvers.Add(new AuthTypeResolveContributor(HttpContextAccessor));
        }
    }


    public class AuthTypeResolveContributor : IClientResolveContributor {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthTypeResolveContributor(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }

        string IClientResolveContributor.ResolveClient() {
            var clientId = "anon";
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out var values)) {
                clientId = values.First();
                // Strip off the actual key or token
                if (clientId != null && clientId.StartsWith("basic")) {
                    clientId = "basic";
                } else if (clientId != null && clientId.StartsWith("bearer")) {
                    clientId = "bearer";
                }
            }
            return clientId;
        }
    }

}