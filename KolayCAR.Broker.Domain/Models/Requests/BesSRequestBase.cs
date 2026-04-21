using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class BesSRequestBase
    {
        public class AvailableVehicleRequest
        {
            public string begin_date { get; set; }
            public string end_date { get; set; }
            public string begin_location { get; set; }
            public string end_location { get; set; }
            public string currency { get; set; }
        }
        public class BesSExtra
        {
            public int id { get; set; }
            public float price { get; set; }
        }

        public class BesSPostReservationRequest
        {
            public string search_vehicle_key { get; set; }
            public string name { get; set; }
            public string code { get; set; }
            public string phone { get; set; }
            public string email { get; set; }
            public string description { get; set; }
            public string currency { get; set; }
            public string? payment_method_code { get; set; }
            public float? payment_method_id { get; set; }
            public float amount_extras { get; set; }
            public float amount_vehicle { get; set; }
            public float amount_drop { get; set; }
            public float amount_final { get; set; }
            public float amount_paid { get; set; }
            public List<BesSExtra> extra { get; set; }
            public string flight_company { get; set; }
            public string departure_airport { get; set; }
            public string landing_airport { get; set; }
            public string flight_number { get; set; }
        }


    }
}
