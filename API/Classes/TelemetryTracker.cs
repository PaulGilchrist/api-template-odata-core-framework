using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Classes {
    // Abstract away Azure Application Insights in case we want to send telemetry to a different or parallel service in the future
    public class TelemetryTracker {

        private TelemetryClient _telemetryClient = null;

        public TelemetryTracker() {
            _telemetryClient = new TelemetryClient();
        }

        public void TrackException(Exception ex, IDictionary<string,string> properties = null, IDictionary<string, double> measurements = null) {
            // Send the exception telemetry:
            _telemetryClient.TrackException(ex, properties, measurements);
        }

        public void TrackTrace(string Message) {
            // Send the exception telemetry:
            _telemetryClient.TrackTrace(Message);
        }

    }

}
