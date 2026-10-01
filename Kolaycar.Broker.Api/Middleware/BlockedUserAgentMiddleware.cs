using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Middleware
{
    public class BlockedUserAgentMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] _blockedUserAgents;

        public BlockedUserAgentMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _blockedUserAgents = configuration
                .GetSection("Security:BlockedUserAgents")
                .Get<string[]>()?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToArray() ?? Array.Empty<string>();
        }

        public async Task Invoke(HttpContext context)
        {
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var isBlocked = _blockedUserAgents.Any(x =>
                userAgent.Contains(x, StringComparison.OrdinalIgnoreCase));

            if (isBlocked)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            await _next(context);
        }
    }
}
