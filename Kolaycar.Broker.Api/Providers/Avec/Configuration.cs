using System.Collections.Generic;

namespace KolayCAR.Broker.API.Providers.Avec
{
    public class Configuration
    {
        public static readonly string ApiBaseUrl = "http://avecccarrentals.com/Sistem_xml/";

        public static Dictionary<string, object> CreateAvecRequestHeader()
        {
            return new Dictionary<string, object>()
            {
                { "Accept", "text/xml; charset=utf-8" }
            };
        }
    }
}
