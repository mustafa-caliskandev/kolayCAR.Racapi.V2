using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KolayCAR.Broker.Infrastructure.Extensions
{
    public static class DictionaryExtensions
    {
        public static string ToQueryString(this IDictionary<string, object> source, bool encode = true) => source != null ? "?" + string.Join("&", source.Select(kvp => string.Format("{0}={1}", encode ? HttpUtility.UrlEncode(kvp.Key) : kvp.Key, encode ? HttpUtility.UrlEncode(kvp.Value.ToStringNullSafe()) : kvp.Value.ToStringNullSafe())).ToArray()) : string.Empty;
    }
}
