using Microsoft.AspNetCore.Mvc;

namespace API.Configuration {
    static class ApiVersions {
        internal static readonly ApiVersion V1 = new ApiVersion(1, 0, "Deprecated");
        internal static readonly ApiVersion V2 = new ApiVersion(2, 0, "Preview");
    }
}