using System.Collections.Generic;
using System.Text;

namespace KolayCAR.Broker.API.Providers.Renteon
{
    public class AuthProvider
    {
        public AuthProvider() { }

        public IDictionary<string, object> GetBasicAuth(Domain.Models.Vendor vendor)
        {
            string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                   .GetBytes(vendor.ApiKey + ":" + vendor.ApiPassword));

            return new Dictionary<string, object>()
                {
                    { "Authorization","Basic " +  encoded}
                };
        }
    }
}
