using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using static Microsoft.AspNetCore.Mvc.Versioning.ApiVersionMapping;

namespace API.Classes {
    /// <summary>
    /// Represents the Swagger/Swashbuckle operation filter used to document the implicit API version parameter.
    /// </summary>
    public class SwaggerDefaultValues : IOperationFilter {
        public void Apply(Operation operation, OperationFilterContext context) {
            var apiDescription = context.ApiDescription;
            var apiVersion = apiDescription.GetApiVersion();
            var model = apiDescription.ActionDescriptor.GetApiVersionModel(Explicit | Implicit);
            operation.Deprecated = model.DeprecatedApiVersions.Contains(apiVersion);
            if (operation.Parameters == null) {
                return;
            }
            foreach (var parameter in operation.Parameters.OfType<NonBodyParameter>()) {
                var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);
                if (parameter.Description == null) {
                    parameter.Description = description.ModelMetadata?.Description;
                }
                if (parameter.Default == null) {
                    parameter.Default = description.DefaultValue;
                }
                parameter.Required |= description.IsRequired;
            }
        }
    }
}