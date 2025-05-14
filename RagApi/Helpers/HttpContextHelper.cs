// RagApi/Helpers/HttpContextHelper.cs
using Microsoft.AspNetCore.Http;

namespace RagApi.Helpers
{
    public static class HttpContextHelper
    {
        // Returns the user ID from the X-User-Id header, or null if not present
        public static string? GetUserIdFromRequest(HttpContext httpContext)
        {
            return httpContext.Request.Headers.TryGetValue("X-User-Id", out var userId)
                ? userId.ToString()
                : null;
        }
    }
}

