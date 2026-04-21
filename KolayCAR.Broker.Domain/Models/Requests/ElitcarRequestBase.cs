using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class ElitcarRequestBase
    {
        public class MasterRequest
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        public class MasterListRequest : MasterRequest
        {
            public string locale { get; set; }
        }

        public class AvailabilityRequest : MasterRequest
        {
            public int pick_id { get; set; }
            public string pick_date_time { get; set; }
            public int drop_id { get; set; }
            public string drop_date_time { get; set; }
            public string country_code { get; set; }
        }


        public class PostReservationRequest : AvailabilityRequest
        {
            public float collected_amount { get; set; }
            public string payment_method { get; set; }
            public string birth_date { get; set; }
            public string phone { get; set; }
            public string id { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public float rental_amount { get; set; }
            public List<ExtraList> services { get; set; }
        }

        public class ExtraList
        {
            public int id { get; set; }
            public int piece { get; set; }
        }

        public class CancelReservation : MasterRequest
        {
            public int id { get; set; }
            public string pnr { get; set; }
        }
    }
}
