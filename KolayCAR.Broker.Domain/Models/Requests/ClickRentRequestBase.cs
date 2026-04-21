using System;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class ClickRentRequestBase
    {

        public class AuthLoginRequest
        {
            public string username { get; set; }
            public string apikey { get; set; }

        }

        public class VehicleRequest
        {
            public DateTime pickupDateTime { get; set; }
            public DateTime returnDateTime { get; set; }
            public int pickupLocationId { get; set; }
            public int returnLocationId { get; set; }

            public string rate { get; set; }
        }

    }
}