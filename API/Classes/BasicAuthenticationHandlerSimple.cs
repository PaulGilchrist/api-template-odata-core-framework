using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace API.Classes {
    public class BasicAuthenticationHandlerSimple : AuthenticationHandler<AuthenticationSchemeOptions> {

        public IConfiguration Configuration { get; }

        public BasicAuthenticationHandlerSimple(
                IConfiguration configuration,
                IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger,
                UrlEncoder encoder,
                ISystemClock clock)
                : base(options, logger, encoder, clock) {
            Configuration = configuration;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
            if (!Request.Headers.ContainsKey("Authorization")) {
                // Authorization header not in request
                return AuthenticateResult.NoResult();
            }
            if (!AuthenticationHeaderValue.TryParse(Request.Headers["Authorization"], out AuthenticationHeaderValue authHeader)) {
                // Invalid Authorization header
                return AuthenticateResult.NoResult();
            }
            if (!"Basic".Equals(authHeader.Scheme, StringComparison.OrdinalIgnoreCase)) {
                // Not Basic authentication header
                return AuthenticateResult.NoResult();
            }
            var token = authHeader.Parameter;
            string[] AuthroizedApiKeys = Configuration.GetValue<string>("Security:AllowedApiKeys").Split(',');
            //Check passed ApiKey against approved list
            foreach (string AuthroizedApiKey in AuthroizedApiKeys) {
                if (AuthroizedApiKey == token) {
                    try {
                        var credentialBytes = Convert.FromBase64String(token);
                        var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
                        var name = credentials[0];
                        var claims = new[] {
                            new Claim(ClaimTypes.Name, name)
                        };
                        var identity = new ClaimsIdentity(claims, Scheme.Name);
                        var principal = new ClaimsPrincipal(identity);
                        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
                    } catch {
                        return AuthenticateResult.Fail("Forbidden");
                    }
                }
            }
            return AuthenticateResult.Fail("Forbidden");
        }

    }

}