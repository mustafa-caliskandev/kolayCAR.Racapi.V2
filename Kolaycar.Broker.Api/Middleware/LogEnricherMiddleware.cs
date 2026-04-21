using KolayCAR.Broker.API.Extensions;
using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Middleware
{
    public class LogEnricherMiddleware
    {
        private readonly RequestDelegate next;

        public readonly IHttpContextAccessor _httpContextAccessor;

        public LogEnricherMiddleware(RequestDelegate next, IHttpContextAccessor httpContextAccessor)
        {
            this.next = next;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task Invoke(HttpContext context)
        {

            //LogContext.PushProperty("ClientIP", context.Connection.RemoteIpAddress.ToString());
            LogContext.PushProperty("ClientIP", _httpContextAccessor.GetClientIp().ToString());

            return next(context);
        }
    }
}
