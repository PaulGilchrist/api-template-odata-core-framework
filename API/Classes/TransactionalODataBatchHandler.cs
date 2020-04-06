using API.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Classes {

    public class TransactionalODataBatchHandler : DefaultODataBatchHandler {
        public override async Task<IList<ODataBatchRequestItem>> ParseBatchRequestsAsync(HttpContext context) {
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
            var requests = await base.ParseBatchRequestsAsync(context);
            var dbContext = context.RequestServices.GetRequiredService<ApiDbContext>();
            return requests.Select(rq => {
                if (rq is ChangeSetRequestItem) {
                    return new TransactionalChangesetRequestItem(rq as ChangeSetRequestItem, dbContext);
                } else {
                    return rq;
                }
            }).ToList();
        }
    }

    public class TransactionalChangesetRequestItem : ODataBatchRequestItem {
        private readonly ChangeSetRequestItem _changeSetRequestItem;
        private readonly ApiDbContext _dbContext;

        public TransactionalChangesetRequestItem(ChangeSetRequestItem changeSetRequestItem, ApiDbContext dbContext) {
            _changeSetRequestItem = changeSetRequestItem;
            _dbContext = dbContext;
        }

        public override async Task<ODataBatchResponseItem> SendRequestAsync(RequestDelegate handler) {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync()) {
                var response = await _changeSetRequestItem.SendRequestAsync(handler) as ChangeSetResponseItem;
                if (response.Contexts.All(c => c.Response.IsSuccessStatusCode())) {
                    transaction.Commit();
                } else {
                    transaction.Rollback();
                }
                return response;
            }
        }
    }

}
