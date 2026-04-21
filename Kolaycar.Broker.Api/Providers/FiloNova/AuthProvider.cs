using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Providers.FiloNova
{
    public class AuthProvider
    {
        public Dictionary<string, object> CreateAuthHeaderWithContentType(Vendor vendor) =>
                new Dictionary<string, object>()
                {
                { "Authorization", $"Basic {EncodingHelper.Base64Encode($"{vendor.ApiKey.Split('|')[0]}:{vendor.ApiPassword}")}"},
                { "Content-Type", "application/json"}
                };

        public Dictionary<string, object> CreateHeaderWithContentType() =>
              new Dictionary<string, object>()
              {
                { "Content-Type", "application/json"}
              };
    }
}
