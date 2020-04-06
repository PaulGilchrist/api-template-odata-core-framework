using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNet.OData.Batch;
using System;

namespace API.Classes {
    public class MyDefaultODataBatchHandler : DefaultODataBatchHandler {
        public override System.Threading.Tasks.Task<System.Collections.Generic.IList<Microsoft.AspNet.OData.Batch.ODataBatchRequestItem>> ParseBatchRequestsAsync(Microsoft.AspNetCore.Http.HttpContext context) {
            RequestTelemetry requestTelemetry = new RequestTelemetry("POST /$batch", DateTime.UtcNow, new TimeSpan(0), "200", true);
            requestTelemetry.Id = context.TraceIdentifier;
            if (context.User.Identity.Name != null) {
                requestTelemetry.Context.User.Id = context.User.Identity.Name;
                requestTelemetry.Context.User.AuthenticatedUserId = context.User.Identity.Name;
            }
            if (context.Items.ContainsKey("RequestBody")) {
                requestTelemetry.Properties.Add("body", (string)context.Items["RequestBody"]);
            }
            TelemetryClient telemetryClient = new TelemetryClient();
            telemetryClient.TrackRequest(requestTelemetry);
            return base.ParseBatchRequestsAsync(context);
        }
    }
}
