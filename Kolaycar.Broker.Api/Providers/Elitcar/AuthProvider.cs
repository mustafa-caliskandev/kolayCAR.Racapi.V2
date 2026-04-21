using System.Collections.Generic;

namespace KolayCAR.Broker.API.Providers.Elitcar
{
    public class AuthProvider
    {
        public Dictionary<string, object> CreateHeaderWithContentType() =>
              new Dictionary<string, object>()
              {
                { "Content-Type", "application/json"}
              };
    }
}
