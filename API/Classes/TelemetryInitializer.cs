// https://github.com/Microsoft/ApplicationInsights-aspnetcore/issues/562

using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace API.Classes {
    public class TelemetryInitializer : ITelemetryInitializer {
        private IHttpContextAccessor _httpContextAccessor;

        public TelemetryInitializer(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Initialize(ITelemetry telemetry) {
            RequestTelemetry requestTelemetry = telemetry as RequestTelemetry;
            if (requestTelemetry != null && _httpContextAccessor.HttpContext != null) {
                if (_httpContextAccessor.HttpContext.User.Identity.Name != null) {
                    requestTelemetry.Context.User.Id = _httpContextAccessor.HttpContext.User.Identity.Name;
                    requestTelemetry.Context.User.AuthenticatedUserId = _httpContextAccessor.HttpContext.User.Identity.Name;
                }
                if (_httpContextAccessor.HttpContext.Items.ContainsKey("RequestBody")) {
                    requestTelemetry.Properties.Add("body", (string)_httpContextAccessor.HttpContext.Items["RequestBody"]);
                }
            }
        }
    }
}
