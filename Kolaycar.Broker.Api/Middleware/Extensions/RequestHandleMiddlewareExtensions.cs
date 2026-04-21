using Microsoft.AspNetCore.Builder;

namespace KolayCAR.Broker.API.Middleware.Extensions
{
    public static class RequestHandleMiddlewareExtensions
    {
        public static void RequestHandleMiddleware(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<RequestHandleMiddleware>();
        }
    }
}
