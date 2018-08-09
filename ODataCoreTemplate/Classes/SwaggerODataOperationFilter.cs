using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OdataCoreTemplate.Classes {
    public class SwaggerODataOperationFilter : IOperationFilter {
        public void Apply(Operation operation, OperationFilterContext context) {
            // Add the standard OData parameters allowed for an controllers when the controller has the [ODataController] attribute and the function has the [EnableQuery] attribute
            var odataControllerAttribute = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                // .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<ODataControllerAttribute>()
                .FirstOrDefault(a => a is ODataControllerAttribute);
            if (odataControllerAttribute != null) {
                if (context.ApiDescription.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase)) {
                    var enableQueryAttribute = context.MethodInfo.GetCustomAttributes(true)
                            .OfType<EnableQueryAttribute>()
                            .FirstOrDefault(a => a is EnableQueryAttribute);
                    if (enableQueryAttribute != null) {
                        List<IParameter> parameters = new List<IParameter>();
                        if (enableQueryAttribute.AllowedQueryOptions.HasFlag(AllowedQueryOptions.Top))
                            parameters.Add(new NonBodyParameter() { Name = "$top", Description = "Limits the number of items to be returned", Required = false, Type = "integer", In = "query" });
                        if (enableQueryAttribute.AllowedQueryOptions.HasFlag(AllowedQueryOptions.Skip))
                            parameters.Add(new NonBodyParameter() { Name = "$skip", Description = "Skips the first /n/ items of the queried collection from the result", Required = false, Type = "integer", In = "query" });
                        if (enableQueryAttribute.AllowedQueryOptions.HasFlag(AllowedQueryOptions.Filter))
                            parameters.Add(new NonBodyParameter() { Name = "$filter", Description = "Restricts the set of items returned", Required = false, Type = "string", In = "query" });
                        if (enableQueryAttribute.AllowedQueryOptions.HasFlag(AllowedQueryOptions.Select))
                            parameters.Add(new NonBodyParameter() { Name = "$select", Description = "Restricts the properties returned", Required = false, Type = "string", In = "query" });
                        if (enableQueryAttribute.AllowedQueryOptions.HasFlag(AllowedQueryOptions.OrderBy))
                            parameters.Add(new NonBodyParameter() { Name = "$orderby", Description = "Sorts the returned items by these properties (asc or desc)", Required = false, Type = "string", In = "query" });
                        if (enableQueryAttribute.AllowedQueryOptions.HasFlag(AllowedQueryOptions.Expand))
                            parameters.Add(new NonBodyParameter() { Name = "$expand", Description = "Expands navigation properties", Required = false, Type = "string", In = "query" });
                        if (enableQueryAttribute.AllowedQueryOptions.HasFlag(AllowedQueryOptions.Count))
                            parameters.Add(new NonBodyParameter() { Name = "$count", Description = "Retrieves the total count of items as an attribute in the results.", Required = false, Type = "boolean", In = "query" });
                        operation.Parameters = parameters;
                    }
                }
            }
        }
    }
}
