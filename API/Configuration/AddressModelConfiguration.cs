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
            builder.EntitySet<Address>("addresses")
                .EntityType
                .Filter()
                .Count()
                .Expand()
                .OrderBy()
                .Page()
                .Select()
                //.HasKey(o => o.Id)
                .HasMany(a => a.Users)
                .Expand();

            //if (apiVersion < ApiVersions.V2) {
            //    address.Ignore(o => o.Street2);
            //}

            //if (apiVersion >= ApiVersions.V1) {
            //    address.Collection.Function("MostExpensive").ReturnsFromEntitySet<Address>("addresses");
            //}

            //if (apiVersion >= ApiVersions.V2) {
            //    address.Action("Rate").Parameter<int>("rating");
            //}
        }
    }
}