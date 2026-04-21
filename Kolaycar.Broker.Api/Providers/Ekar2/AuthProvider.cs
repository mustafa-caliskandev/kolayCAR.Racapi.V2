using KolayCAR.Broker.Domain.Models;
using System.Collections.Generic;
using System.Text;

namespace KolayCAR.Broker.API.Providers.Ekar2
{
    public class AuthProvider
    {
        string baseUrl;
        public AuthProvider(string ApiBaseUrl)
        {
            baseUrl = ApiBaseUrl;
        }

        public IDictionary<string, object> GetHeaders(Vendor vendor)
        {
            string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                               .GetBytes(vendor.ApiKey + ":" + vendor.ApiPassword));

            return new Dictionary<string, object>()
                    {
                        { "Authorization","Basic "+encoded }
                    };
        }
    }
}
