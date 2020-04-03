using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace API.Classes {
    public class CustomRateLimitMiddleware : ClientRateLimitMiddleware {
        public CustomRateLimitMiddleware(RequestDelegate next, IOptions<ClientRateLimitOptions> options, IRateLimitCounterStore counterStore, IClientPolicyStore policyStore, IRateLimitConfiguration config, ILogger<ClientRateLimitMiddleware> logger) : base(next, options, counterStore, policyStore, config, logger)
        {
        }

        public override ClientRequestIdentity ResolveIdentity(HttpContext httpContext) {
            var clientRequestIdentity = base.ResolveIdentity(httpContext);
            var clientId = clientRequestIdentity.ClientId?.ToLower();
            // Strip off the actual key or token
            if (clientId != null && clientId.StartsWith("basic")) {
                clientRequestIdentity.ClientId = "basic";
            } else if (clientId != null && clientId.StartsWith("bearer")) {
                clientRequestIdentity.ClientId = "bearer";
            }
            return clientRequestIdentity;
        }

    }

}