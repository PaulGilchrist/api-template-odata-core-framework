using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Classes {
    public class ODataFilterQueryValidator : FilterQueryValidator {

        public ODataFilterQueryValidator(DefaultQuerySettings defaultQuerySettings) : base (defaultQuerySettings) { }

        public override void ValidateNavigationPropertyNode(QueryNode sourceNode, IEdmNavigationProperty navigationProperty, ODataValidationSettings settings) {
            //Do not limit node count
            settings.MaxNodeCount = 100000;
            base.ValidateNavigationPropertyNode(sourceNode, navigationProperty, settings);
        }
    }
}
