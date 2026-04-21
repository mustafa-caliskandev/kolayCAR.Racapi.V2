using System.Collections.Generic;
using System.Net.Http;

namespace KolayCAR.Broker.Infrastructure.Extensions
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
                    client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value.ToStringNullSafe());
                }
            }

            return client;
        }
    }
}
