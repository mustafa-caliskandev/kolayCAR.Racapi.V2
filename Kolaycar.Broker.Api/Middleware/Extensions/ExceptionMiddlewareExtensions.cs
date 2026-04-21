using Microsoft.AspNetCore.Builder;

namespace KolayCAR.Broker.API.Middleware.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ExceptionMiddleware(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
