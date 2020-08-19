using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace API.Classes {

    public class SwaggerIgnoreFilter : ISchemaFilter {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context) {
            if (schema?.Properties == null || schema.Properties.Count == 0) {
                return;
            }
            // Hide all models except enums to reduce the browser memory consumption from Swagger UI showing deep nested models
            var excludedList = context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(t => t.PropertyType.FullName.Contains("API.Models") && !t.PropertyType.FullName.Contains("Enums"))
                .Select(m => (m.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? m.Name.ToCamelCase()));
            foreach (var excludedName in excludedList) {
                if (schema.Properties.ContainsKey(excludedName))
                    schema.Properties.Remove(excludedName);
            }
        }
    }

    internal static class StringExtensions {
        internal static string ToCamelCase(this string value) {
            if (string.IsNullOrEmpty(value)) return value;
            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }
    }

}
