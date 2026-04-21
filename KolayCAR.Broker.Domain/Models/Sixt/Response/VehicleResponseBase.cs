using Newtonsoft.Json;

namespace KolayCAR.Broker.Domain.Models.Sixt.Response
{
    public class VehicleResponseBase
    {
        [JsonProperty("?xml")]
        public Xml Xml { get; set; }
        public SIXTTURKEYWEBSERVICESFORVEHICLELIST SIXTTURKEYWEBSERVICES { get; set; }
    }
}
