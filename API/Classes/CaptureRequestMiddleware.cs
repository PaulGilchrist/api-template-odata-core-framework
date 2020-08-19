using API.Classes;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace API.Classes {
    public class CaptureRequestMiddleware {
        private readonly RequestDelegate _next;
        private readonly string _loggingLevel;
        private readonly TelemetryTracker _telemetryTracker;

        public CaptureRequestMiddleware(RequestDelegate next, string loggingLevel, TelemetryTracker telemetryTracker) {
            _loggingLevel = loggingLevel;
            _next = next;
            _telemetryTracker = telemetryTracker;
        }

        public async Task Invoke(HttpContext context) {
            try {
                context.Request.EnableBuffering();
                if ((context.Request.ContentLength > 0) && (_loggingLevel == "High")) {
                    using (var reader = new StreamReader(context.Request.Body)) {
                        var requestBody = await reader.ReadToEndAsync();
                        context.Items.Add("RequestBody", requestBody);
                        context.Request.Body.Seek(0, SeekOrigin.Begin);
                        await _next(context);
                    }
                } else {
                    await _next(context);
                }
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
            }
        }
    }

}
