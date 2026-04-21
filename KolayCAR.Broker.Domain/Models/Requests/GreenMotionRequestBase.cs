using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class GreenMotionRequestBase
    {
        public GreenMotionRequestBaseScope gm_webservice { get; set; }

        public class GreenMotionRequestBaseScope
        {
            public GreenMotionRequestHeader header { get; set; }
            public object request { get; set; }
        }

        public class GreenMotionRequestHeader
        {
            public string username { get; set; }
            public string password { get; set; }
            public string version { get; set; }
        }

        public class GreenMotionGetLocationsRequest
        {
            public int country_id { get; set; }
            public string language { get; set; }
            [JsonProperty("@type")]
            public string type { get; set; }
        }

        public class GreenMotionGetLocationDetailRequest
        {
            public int location_id { get; set; }
            [JsonProperty("@type")]
            public string type { get; set; }
        }

        public class GreenMotionGetVehiclesRequest
        {
            public int location_id { get; set; }
            public int dropoff_location_id { get; set; }
            public string start_date { get; set; }
            public string start_time { get; set; }
            public string end_date { get; set; }
            public string end_time { get; set; }
            public int age { get; set; }
            public string fuel { get; set; }
            public string currency { get; set; }
            public string userid { get; set; }
            public string full_credit { get; set; }
            public string username { get; set; }
            [JsonProperty("@type")]
            public string type { get; set; }
        }

        public class GreenMotionPostReservationRequest
        {
            public int location_id { get; set; }
            public int dropoff_location_id { get; set; }
            public string start_date { get; set; }
            public string start_time { get; set; }
            public string end_date { get; set; }
            public string end_time { get; set; }
            public string vehicle_id { get; set; }
            public string vehicle_total { get; set; }
            public string currency { get; set; }
            public GreenMotionGreenMotionReservationOptionList options { get; set; }
            public string grand_total { get; set; }
            public GreenMotionCustomerInfo cust_info { get; set; }
            public string paymentHandlerRef { get; set; }
            public string payment_type { get; set; }
            public string quoteid { get; set; }
            [JsonProperty("@type")]
            public string type { get; set; }
            public string full_credit { get; set; }
        }

        public class GreenMotionGreenMotionReservationOptionList
        {
            public List<GreenMotionReservationOption> option { get; set; }
        }

        public class GreenMotionReservationOption
        {
            [JsonProperty("@id")]
            public int id { get; set; }
            [JsonProperty("@option_qty")]
            public int option_qty { get; set; }
            [JsonProperty("@option_total")]
            public string option_total { get; set; }
        }

        public class GreenMotionCustomerInfo
        {
            public string firstname { get; set; }
            public string lastname { get; set; }
            public int age { get; set; }
            public string telephone { get; set; }
            public string mobile { get; set; }
            public string email { get; set; }
            public string flight_no { get; set; }
            public string address1 { get; set; }
            public string address2 { get; set; }
            public string address3 { get; set; }
            public string city { get; set; }
            public string county { get; set; }
            public string postcode { get; set; }
            public string country { get; set; }
            public string bplace { get; set; }
            public string bdate { get; set; }
            public string idno { get; set; }
            public string idplace { get; set; }
            public string idissue { get; set; }
            public string idexp { get; set; }
            public string licno { get; set; }
            public string licissue { get; set; }
            public string licplace { get; set; }
            public string licexp { get; set; }
            public string idurl { get; set; }
            public string id_rear_url { get; set; }
            public string licurl { get; set; }
            public string lic_rear_url { get; set; }
            public string verification_response { get; set; }
            public string custimage { get; set; }
            public string dvlacheckcode { get; set; }
        }

        public class GreenMotionPostCancelReservationRequest
        {
            public string location_id { get; set; }
            public string booking_ref { get; set; }
            [JsonProperty("@type")]
            public string type { get; set; }
        }
    }
}
