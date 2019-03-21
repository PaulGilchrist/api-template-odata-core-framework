using System;

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
