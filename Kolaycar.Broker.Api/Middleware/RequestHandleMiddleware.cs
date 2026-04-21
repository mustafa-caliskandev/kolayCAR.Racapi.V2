using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Middleware
{
    public class RequestHandleMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestHandleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // /reservation her zaman loglanmalı
            Serilog.Log.ForContext("QueryString", context.Request.QueryString.Value)
                .Debug("{QueryString}", context.Request.QueryString.Value);
            await _next.Invoke(context);
        }
    }
}
