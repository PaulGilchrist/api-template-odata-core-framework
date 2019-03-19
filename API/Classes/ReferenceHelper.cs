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

        //public static TKey GetKeyFromUri<TKey>(HttpRequestMessage request, Uri uri) {
        //    if (uri==null) {
        //        throw new ArgumentNullException("uri");
        //    }
        //    var urlHelper = request.GetUrlHelper()??new UrlHelper(request);
        //    var pathHandler = (IODataPathHandler)request.GetRequestContainer().GetService(typeof(IODataPathHandler));
        //    string serviceRoot = urlHelper.CreateODataLink(request.ODataProperties().RouteName, pathHandler, new List<ODataPathSegment>());
        //    var odataPath = pathHandler.Parse(serviceRoot, uri.OriginalString, request.GetRequestContainer());
        //    var keySegment = odataPath.Segments.OfType<KeySegment>().FirstOrDefault();
        //    if (keySegment==null) {
        //        throw new InvalidOperationException("The link does not contain a key.");
        //    }

        //    var value = keySegment.Keys.FirstOrDefault().Value;
        //    return (TKey)value;
        //}

        //public static Dictionary<string, int> GetNestedKeyFromUri<TKey>(HttpRequestMessage request, Uri uri) {
        //    if (uri==null) {
        //        throw new ArgumentNullException("uri");
        //    }
        //    var urlHelper = request.GetUrlHelper()??new UrlHelper(request);
        //    var pathHandler = (IODataPathHandler)request.GetRequestContainer().GetService(typeof(IODataPathHandler));
        //    string serviceRoot = urlHelper.CreateODataLink(
        //        request.ODataProperties().RouteName,
        //        pathHandler, new List<ODataPathSegment>());
        //    var odataPath = pathHandler.Parse(serviceRoot, uri.OriginalString, request.GetRequestContainer());
        //    var keySegment = odataPath.Segments.OfType<KeySegment>();
        //    if (keySegment==null) {
        //        throw new InvalidOperationException("The link does not contain a key.");
        //    }
        //    if (keySegment.Count()<=1) {
        //        return default;
        //    }
        //    var keyValue = (int)keySegment.ToList()[1].Keys.FirstOrDefault().Value;
        //    var identifier = keySegment.ToList()[1].Identifier;
        //    var value = new Dictionary<string, int>();
        //    value.Add(identifier, keyValue);
        //    return value;
        //}


    }
}
