using API.Configuration;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.OData.UriParser;
using ODataCoreTemplate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace API.Classes {
    public class ReferenceHelper {


        public static string GetKeyFromUrl(Uri uri) {
            if (uri==null) {
                throw new ArgumentNullException("uri");
            }
            var finalSegment = uri.Segments[uri.Segments.Length-1];
            var startIndex = finalSegment.LastIndexOf("(");
            var endIndex = finalSegment.LastIndexOf(")");
            if (startIndex!=-1 && endIndex!=-1) {
                return finalSegment.Substring(startIndex+1, endIndex-startIndex-1);
            } else {
                return null;
            }
        }

    }
}
