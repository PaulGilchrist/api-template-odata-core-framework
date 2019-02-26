namespace API.Configuration {
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNetCore.Mvc;
    using ODataCoreTemplate.Models;

    /// <summary>
    /// Represents the model configuration for addresses.
    /// </summary>
    public class AddressModelConfiguration : IModelConfiguration {
        /// <summary>
        /// Applies model configurations using the provided builder for the specified API version.
        /// </summary>
        /// <param name="builder">The <see cref="ODataModelBuilder">builder</see> used to apply configurations.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the <paramref name="builder"/>.</param>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion) {
            var address = builder.EntitySet<Address>("addresses").EntityType;
            address.HasKey(o => o.Id)
                .HasMany(a => a.Users)
                .Filter()
                .Count()
                .Expand()
                .OrderBy()
                .Page()
                .Select()
                .Expand();
            // Eample of how we can remove a field in the data model that may still exist in the database, supporting zero downtime deployments
            //     Adding a property would not be considered a breaking change and not warrant a new ApiVersion
            if (apiVersion > ApiVersions.V2) {
                address.Ignore(o => o.StreetName2);
            }

        }
    }
}