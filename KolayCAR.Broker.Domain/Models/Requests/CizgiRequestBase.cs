using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class CizgiRequestBase
    {
        public class AuthLoginRequest
        {
            public string brokerid { get; set; }
            public string secret { get; set; }
        }
        public class AvailabilityRequest
        {
            public string pickup_date { get; set; }
            public int pickup_location { get; set; }
            public string dropoff_date { get; set; }
            public int dropoff_location { get; set; }
            public List<object> rate_codes { get; set; }
        }

        public class PostReservationRequest : AvailabilityRequest
        {
            public string reference_no { get; set; }
            public int reference_id { get; set; }
            public string name { get; set; }
            public string surname { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string notes { get; set; }
            public string birth_date { get; set; }
            public string passport_no { get; set; }
            public string citizenship_no { get; set; }
            public string driver2_name { get; set; }
            public string driver2_surname { get; set; }
            public string driver2_email { get; set; }
            public string driver2_phone { get; set; }
            public string driver2_passport_no { get; set; }
            public string driver2_citizenship_no { get; set; }
            public int car_id { get; set; }
            public string flight_number { get; set; }
            public float payment_made { get; set; }
            public Dictionary<string, int> packages { get; set; }
            public string search_id { get; set; }
        }

        public class ExtraList
        {
            public string value { get; set; }
        }

        public class CancelReservationRequest
        {
            public string reference_no { get; set; }
            public string reason { get; set; }
        }
    }
}
