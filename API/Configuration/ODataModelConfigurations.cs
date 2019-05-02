using API.Models;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Mvc;

namespace API.Configuration {
    /// <summary>
    /// Represents the model configuration for all configurations.
    /// </summary>
    public class ODataModelConfigurations : IModelConfiguration {
        /// <summary>
        /// Applies model configurations using the provided builder for the specified API version.
        /// </summary>
        /// <param name="builder">The <see cref="ODataModelBuilder">builder</see> used to apply configurations.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the <paramref name="builder"/>.</param>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion) {
            builder.Namespace = "ApiTemplate";
            builder.ContainerName = "ApiTemplateContainer";
            // Called once for each apiVersion, so this is the best place to define the EntiitySet differences from version to version
            var user = builder.EntitySet<User>("users").EntityType;
            user.Count().Expand(5).Filter().OrderBy().Page().Select();

            var address = builder.EntitySet<Address>("addresses").EntityType;
            address.Count().Expand(5).Filter().OrderBy().Page().Select();

            //Example of how we can remove a field in the data model that may still exist in the database, supporting zero downtime deployments
            //     Adding a property would not be considered a breaking change and not warrant a new ApiVersion
            if (apiVersion>=ApiVersions.V2) {
                user.Ignore(o => o.MiddleName);
                address.Ignore(o => o.StreetName2);
            }
        }
    }
}