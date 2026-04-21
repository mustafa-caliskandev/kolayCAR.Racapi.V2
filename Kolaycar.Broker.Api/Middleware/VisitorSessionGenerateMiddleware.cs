using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Middleware
{
    public class VisitorSessionGenerateMiddleware
    {
        private readonly RequestDelegate _next;

        public VisitorSessionGenerateMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext httpContext)
        {
            var sessionId = httpContext.Session.GetString("user-code");
            if (string.IsNullOrEmpty(sessionId))
            {
                httpContext.Session.SetString("user-code", CreateRandomString());
            }
            await _next(httpContext);
        }

        public static string CreateRandomString()
        {
            var allowed = "ABCDEFGHIJKLMONOPQRSTUVWXYZabcdefghijklmonopqrstuvwxyz0123456789";
            var strlen = 11;
            var randomChars = new char[strlen];

            for (var i = 0; i < strlen; i++)
            {
                randomChars[i] = allowed[RandomNumberGenerator.GetInt32(0, allowed.Length)];
            }

            return new string(randomChars);
        }
    }
    public static class VisitorSessionGenerateMiddlewareExtensions
    {
        public static IApplicationBuilder UseVisitorSessionGenerateMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<VisitorSessionGenerateMiddleware>();
        }
    }
}
