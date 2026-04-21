using System.Collections.Generic;
using System.Net.Http;

namespace KolayCAR.Broker.Infrastructure.Extensions
{
    public static class FormUrlEncodedContentExtensions
    {
        public static FormUrlEncodedContent SetHeaders(this FormUrlEncodedContent client, IDictionary<string, object> headers)
        {
            if (headers != null)
            {
                client.Headers.Clear();

                foreach (var header in headers)
                {
                    client.Headers.TryAddWithoutValidation(header.Key, header.Value.ToStringNullSafe());
                }

            }

            return client;
        }
    }
}
