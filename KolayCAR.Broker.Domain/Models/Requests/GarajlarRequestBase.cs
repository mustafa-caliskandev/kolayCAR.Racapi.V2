using KolayCAR.Broker.Domain.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class GarajlarRequestBase
    {
        public class AuthRequest 
        {
            public string email { get; set; }
            public string password { get; set; }
        }
        public class AvailabilityVehiclesRequest
        {
            public string startLocationCode { get; set; }
            public string endLocationCode { get; set; }
            public string startDateTime { get; set; }
            public string endDateTime { get; set; }
        }
        public class ReservationRequest
        {
           public PersonalInfo personal_info { get; set; }
           public ReservationInfo reservation_info { get; set; }
           public List<Extras> extras { get; set; }
        }
        public class PersonalInfo
        {
            public string address { get; set; }
            public string birthday { get; set; }
            public string city { get; set; }
            public string country { get; set; }
            public string district { get; set; }
            public string email { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string telephone { get; set; }
            public string identity { get; set; }
            public string identity_type { get; set; } //1 T.C. no, 3 Pasaport No
        }
        public class ReservationInfo
        {
            public string campaign_id { get; set; }
            public string contract_number { get; set; }
            public string endDateTime { get; set; }
            public string endLocationCode { get; set; }
            public int main_group_id { get; set; }
            public int main_rule_id { get; set; }
            public string startDateTime { get; set; }
            public string startLocationCode { get; set; }
            public int sub_group_id { get; set; }
            public string sub_group_short_name { get; set; }
        }
        public class Extras
        {
            public string code { get; set; }
        }
        public class CancelReservationRequest
        {
            public int reservationId { get; set; }
        }
    }
}
