namespace API.Configuration {
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNetCore.Mvc;
    using ODataCoreTemplate.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents the model configuration for users.
    /// </summary>
    public class UserModelConfiguration : IModelConfiguration {
        /// <summary>
        /// Applies model configurations using the provided builder for the specified API version.
        /// </summary>
        /// <param name="builder">The <see cref="ODataModelBuilder">builder</see> used to apply configurations.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the <paramref name="builder"/>.</param>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion) {
            // Called once for each apiVersion, so this is the best place to define the EntiitySet differences from version to version
            builder.EntitySet<User>("users")
                .EntityType
                .HasKey(o => o.Id)
                .Filter()
                .Count()
                .Expand()
                .OrderBy()
                .Page()
                .Select()
                .HasMany(u => u.Addresses)
                .Expand();
 
            //if (apiVersion < ApiVersions.V2) {
            //    user.Ignore(o => o.MiddleName);
            //}
            //if (apiVersion >= ApiVersions.V1) {
            //    user.Collection.Function("MostExpensive").ReturnsFromEntitySet<User>("users");
            //}
            //if (apiVersion >= ApiVersions.V2) {
            //    user.Action("Rate").Parameter<int>("rating");
            //}
        }
    }
}