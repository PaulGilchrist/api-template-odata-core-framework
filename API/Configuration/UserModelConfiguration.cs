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
                .HasMany(u => u.Addresses)
                .Filter()
                .Count()
                .Expand()
                .OrderBy()
                .Page()
                .Select()
                .Expand();
            builder.Function("users").Returns<IEnumerable<User>>().Parameter<UserList>("userList"); // Only works for POST
            //Eample of how we can remove a field in the data model that may still exist in the database, supporting zero downtime deployments
            //     Adding a property would not be considered a breaking change and not warrant a new ApiVersion
            //if (apiVersion > ApiVersions.V1) {
            //    user.Ignore(o => o.MiddleName);
            //}
        }
    }
}