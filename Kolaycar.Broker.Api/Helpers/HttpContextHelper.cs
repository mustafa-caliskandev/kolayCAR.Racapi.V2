using KolayCAR.Broker.API.Extensions;
using Microsoft.AspNetCore.Http;

namespace KolayCAR.Broker.API.Helpers
{
    public class HttpContextHelper
    {
        public readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextHelper(
            IHttpContextAccessor httpContextAccessor
            )
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetClientIpAddress()
        {
            return _httpContextAccessor.GetClientIp();
        }
    }
}
