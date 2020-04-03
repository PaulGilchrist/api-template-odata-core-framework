using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace API.Classes {
    public class CustomRateLimitConfiguration : IRateLimitConfiguration {
        protected readonly IpRateLimitOptions IpRateLimitOptions;
        protected readonly ClientRateLimitOptions ClientRateLimitOptions;
        protected readonly ClientRateLimitPolicies ClientRateLimitPolicies;
        protected readonly IHttpContextAccessor HttpContextAccessor;
        public IList<IClientResolveContributor> ClientResolvers { get; } = new List<IClientResolveContributor>();
        public IList<IIpResolveContributor> IpResolvers { get; } = new List<IIpResolveContributor>();
        public virtual ICounterKeyBuilder EndpointCounterKeyBuilder { get; } = new PathCounterKeyBuilder();
        public virtual Func<double> RateIncrementer { get; } = () => 1;

        public CustomRateLimitConfiguration(IHttpContextAccessor httpContextAccessor, IOptions<IpRateLimitOptions> ipOptions, IOptions<ClientRateLimitOptions> clientOptions, IOptions<ClientRateLimitPolicies> clientPolicies) {
            IpRateLimitOptions = ipOptions?.Value;
            ClientRateLimitOptions = clientOptions?.Value;
            ClientRateLimitPolicies = clientPolicies?.Value;
            HttpContextAccessor = httpContextAccessor;
            ClientResolvers = new List<IClientResolveContributor>();
            IpResolvers = new List<IIpResolveContributor>();
            RegisterResolvers();
        }

        protected virtual void RegisterResolvers() {
            ClientResolvers.Add(new AuthTypeResolveContributor(HttpContextAccessor, ClientRateLimitOptions, ClientRateLimitPolicies));
        }
    }

    public class AuthTypeResolveContributor : IClientResolveContributor {
        private readonly ClientRateLimitOptions _clientOptions;
        private readonly ClientRateLimitPolicies _clientPolicies;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthTypeResolveContributor(IHttpContextAccessor httpContextAccessor, ClientRateLimitOptions clientOptions, ClientRateLimitPolicies clientPolicies) {
            _clientOptions = clientOptions;
            _clientPolicies = clientPolicies;
            _httpContextAccessor = httpContextAccessor;
        }

        string IClientResolveContributor.ResolveClient() {
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(_clientOptions.ClientIdHeader, out var values)) {
                var clientId = values.First();
                if (_clientPolicies.ClientRules.FirstOrDefault(r => r.ClientId.Equals(clientId, StringComparison.OrdinalIgnoreCase)) == null) {
                    // The exact clientId was not found in ClientRateLimitPolicies so we will not modify the clientId to generalize the auth type
                    if (clientId.StartsWith("basic", StringComparison.OrdinalIgnoreCase)) {
                        // Basic authorization header(applications) will use the clientId rules pooling all users of that application together
                        clientId = "basic";
                    } else if (clientId.StartsWith("bearer", StringComparison.OrdinalIgnoreCase)) {
                        // Bearer authorization header(individual user) will use the clientId rules much stricter than for an entire application
                        clientId = "bearer";
                    }
                }
                return clientId;
            } else {
                // No ClientIdHeader was found so the user is anonymous and we will use the general rules
                return "anon";
            }
        }
    }

}
