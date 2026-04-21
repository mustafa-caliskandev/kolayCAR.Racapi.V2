using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Providers.Garenta
{
    public class AuthProvider
    {
        public Dictionary<string, object> CreateAuthHeaderWithContentType(Vendor vendor) =>
                new Dictionary<string, object>()
                {
                    { "Authorization", $"Basic {EncodingHelper.Base64Encode($"{vendor.ApiKey}:{vendor.ApiPassword}")}"},
                    { "Content-Type", "application/json"}
                };
    }
}
