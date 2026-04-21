using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace KolayCAR.Broker.API.Extensions
{
    public static class HttpClientExtensions
    {
        public static HttpClient SetHeaders(this HttpClient client, IDictionary<string, object> headers)
        {
            if (headers != null)
            {
                client.DefaultRequestHeaders.Clear();

                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value != null ? header.Value.ToString() : string.Empty);
                }
            }

            return client;
        }

        public static string GetClientLanguage(this HttpClient client)
        {
            var maxQuality = client.DefaultRequestHeaders.AcceptLanguage
                                                         .Select(x => new { x.Value, Quality = x.Quality ?? 1 })
                                                         .OrderByDescending(x => x.Quality)
                                                         .FirstOrDefault();
            return maxQuality?.Value ?? "";

            //return client.DefaultRequestHeaders.AcceptLanguage.MaxBy(x => x.Quality ?? 1)?.Value ?? "";
        }

        public static string GetClientIp(this IHttpContextAccessor accessor)
        {
            if (!string.IsNullOrEmpty(accessor.HttpContext.Request.Headers["CF-CONNECTING-IP"]))
                return accessor.HttpContext.Request.Headers["CF-CONNECTING-IP"];

            var ipAddress = accessor.HttpContext.Request.Headers["HTTP_X_FORWARDED_FOR"].ToString();

            if (string.IsNullOrEmpty(ipAddress)) return accessor.HttpContext.Connection.RemoteIpAddress.ToString();

            var addresses = ipAddress.Split(',');
            return addresses.Length != 0 ? addresses.Last() : accessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }
    }
}
