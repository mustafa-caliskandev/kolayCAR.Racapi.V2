using System.Collections.Generic;

namespace KolayCAR.Broker.API.Providers._5S
{
    public class Configuration
    {
        public static Dictionary<string, object> CreateHeaderWithAuth(string apiKey)
        {
            return new Dictionary<string, object>
            {
                { "Content-Type", "application/json"},
                { "Accept", "application/json" },
                { "API-KEY", apiKey}
            };
        }
    }
}
