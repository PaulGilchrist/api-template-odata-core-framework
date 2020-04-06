using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
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
        protected readonly IClientPolicyStore ClientPolicyStore;
        public IList<IClientResolveContributor> ClientResolvers { get; } = new List<IClientResolveContributor>();
        public IList<IIpResolveContributor> IpResolvers { get; } = new List<IIpResolveContributor>();
        public virtual ICounterKeyBuilder EndpointCounterKeyBuilder { get; } = new PathCounterKeyBuilder();
        public virtual Func<double> RateIncrementer { get; } = () => 1;

        public CustomRateLimitConfiguration(IHttpContextAccessor httpContextAccessor, IClientPolicyStore clientPolicyStore, IOptions<IpRateLimitOptions> ipOptions, IOptions<ClientRateLimitOptions> clientOptions, IOptions<ClientRateLimitPolicies> clientPolicies) {            IpRateLimitOptions = ipOptions?.Value;
            ClientRateLimitOptions = clientOptions?.Value;
            ClientRateLimitPolicies = clientPolicies?.Value;
            HttpContextAccessor = httpContextAccessor;
            ClientPolicyStore = clientPolicyStore;
            ClientResolvers = new List<IClientResolveContributor>();
            IpResolvers = new List<IIpResolveContributor>();
            RegisterResolvers();
        }

        protected virtual void RegisterResolvers() {
            ClientResolvers.Add(new AuthTypeResolveContributor(HttpContextAccessor, ClientPolicyStore, ClientRateLimitOptions, ClientRateLimitPolicies));
        }
    }

    public class AuthTypeResolveContributor : IClientResolveContributor {
        private readonly ClientRateLimitOptions _clientOptions;
        private readonly ClientRateLimitPolicies _clientPolicies;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClientPolicyStore _clientPolicyStore;

        public AuthTypeResolveContributor(IHttpContextAccessor httpContextAccessor, IClientPolicyStore clientPolicyStore, ClientRateLimitOptions clientOptions, ClientRateLimitPolicies clientPolicies) {
            _clientOptions = clientOptions;
            _clientPolicies = clientPolicies;
            _httpContextAccessor = httpContextAccessor;
            _clientPolicyStore = clientPolicyStore;
        }

        string IClientResolveContributor.ResolveClient() {
            // General rules will be the default for individual users (bearer tokens), and the "basic" client rules will be for applications (basic tokens)
            if (!_clientOptions.ClientIdHeader.Equals("Authorization", StringComparison.OrdinalIgnoreCase) && _httpContextAccessor.HttpContext.Request.Headers.TryGetValue(_clientOptions.ClientIdHeader, out var headerValues)) {
                // The requestor is using a header other than "Authorization" so just return the clientId unchanged
                return headerValues.First();
            } else if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out var authValues)) {
                var clientId = authValues.First();
                if (_clientPolicies.ClientRules.FirstOrDefault(r => r.ClientId.Equals(clientId, StringComparison.OrdinalIgnoreCase)) != null) {
                    // There is a specific client policy rule already set for for this "Authorization" client so just return the clientId unchanged
                } else {
                    if (clientId.StartsWith("basic", StringComparison.OrdinalIgnoreCase)) {
                        var rule = _clientPolicies.ClientRules.FirstOrDefault(r => r.ClientId.Equals("basic", StringComparison.OrdinalIgnoreCase));
                        if (rule != null) {
                            // Set this application to have the same rules as the default application rules 
                            _clientPolicyStore.SetAsync($"{_clientOptions.ClientPolicyPrefix}_{clientId}", new ClientRateLimitPolicy { ClientId = clientId, Rules = rule.Rules }).ConfigureAwait(false);
                        }
                    }
                }
                return clientId;
            } else {
                // No header was found so the user is anonymous and we will use the general rules
                return "anon";
            }
        }

    }

}
