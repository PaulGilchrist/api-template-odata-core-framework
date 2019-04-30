using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace API.Classes {
    public class CaptureRequestMiddleware {
        private readonly RequestDelegate _next;
        public CaptureRequestMiddleware(RequestDelegate next) {
            _next = next;
        }
        public async Task Invoke(HttpContext context) {
            if (context.Request.ContentLength > 0) {
                context.Request.EnableBuffering();
                using (var reader = new StreamReader(context.Request.Body)) {
                    context.Items.Add("RequestBody", reader.ReadToEnd());
                    context.Request.Body.Seek(0, SeekOrigin.Begin);
                    await _next(context);
                }
            } else {
                await _next(context);
            }
        }
    }
    public static class CaptureRequestExtension {
        public static IApplicationBuilder CaptureRequest(this IApplicationBuilder builder) {
            return builder.UseMiddleware<CaptureRequestMiddleware>();
        }
    }
}
