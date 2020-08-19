using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace API.Models {
    public class ODataReference {
        //http://odata.github.io/WebApi/#03-01-routing-abstract
        //Microsoft POST - https://docs.microsoft.com/en-us/aspnet/web-api/overview/odata-support-in-aspnet-web-api/odata-v4/entity-relations-in-odata-v4
        //Microsoft DELETE - https://docs.microsoft.com/en-us/aspnet/web-api/overview/odata-support-in-aspnet-web-api/odata-v4/entity-relations-in-odata-v4#deleting-a-relationship-between-entities

        [Required]
        [JsonProperty(PropertyName = "@odata.id")]
        public Uri uri { get; set; }
    }
}
