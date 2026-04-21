using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Renticar.Request
{
    public class ReservationRequestBody
    {
        public string offerId { get; set; }
        public DriverInfo driverInfo { get; set; }
        public string currency { get; set; }
        public List<Domain.Models.Renticar.Response.Extra> extras { get; set; }
        public string reservationType { get; set; }
    }
    public class DriverInfo
    {
        public Identity identity { get; set; }
        public string name { get; set; }
        public string lastname { get; set; }
        public string birthday { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
    }

    public class Identity
    {
        public string certificateType { get; set; }
        public string value { get; set; }
    }
}
