using Newtonsoft.Json;

namespace KolayCAR.Broker.Domain.Models.Sixt.Response
{
    public class ReservationSaveResponseBase
    {
        [JsonProperty("?xml")]
        public Xml Xml { get; set; }
        public SIXTTURKEYWEBSERVICES SIXTTURKEYWEBSERVICES { get; set; }
    }

    public class CUSTOMER
    {
        public string NAME { get; set; }
        public string SURNAME { get; set; }
    }

    public class DOCUMENT
    {
        public string LINK { get; set; }
    }

    public class RESERVATIONSTATUS
    {
        [JsonProperty("@token")]
        public string Token { get; set; }
        public string RESERVATIONCODE { get; set; }
        public CUSTOMER CUSTOMER { get; set; }
        public VEHICLE VEHICLE { get; set; }
        public string DAILYPRICE { get; set; }
        public string EXTRASPRICE { get; set; }
        public string TOTALPRICE { get; set; }
        public string ONEWAY { get; set; }
        public string RENTALDAYS { get; set; }
        public PAYMENTOPTIONS PAYMENTOPTIONS { get; set; }
        public AVAILABILITY AVAILABILITY { get; set; }
        public PICKUP PICKUP { get; set; }
        public RETURN RETURN { get; set; }
        public DOCUMENT DOCUMENT { get; set; }
    }

}
