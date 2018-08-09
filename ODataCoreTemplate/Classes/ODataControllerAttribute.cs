using System;
using System.Collections.Generic;
using System.Text;

namespace OdataCoreTemplate.Classes {
    [AttributeUsage(AttributeTargets.Class)]
    public class ODataControllerAttribute : Attribute {
        // Place this attribute on an OData controller to allow Swagger to document its endpoints
        public Type EntityType { get; set; }

        public ODataControllerAttribute(Type entityType) {
            this.EntityType = entityType;
        }
    }
}
