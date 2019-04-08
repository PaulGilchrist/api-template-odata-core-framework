// https://github.com/Microsoft/ApplicationInsights-aspnetcore/issues/562

using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace API.Classes {
    public class TelemetryInitializer : ITelemetryInitializer {
        IHttpContextAccessor httpContextAccessor;

        public TelemetryInitializer(IHttpContextAccessor httpContextAccessor) {
            this.httpContextAccessor = httpContextAccessor;
        }

        public void Initialize(ITelemetry telemetry) {
            var requestTelemetry = telemetry as RequestTelemetry;
            if (requestTelemetry != null && httpContextAccessor.HttpContext != null && httpContextAccessor.HttpContext.User.Identity.Name != null) {
                requestTelemetry.Context.User.Id = httpContextAccessor.HttpContext.User.Identity.Name;
            }
        }
    }
}
