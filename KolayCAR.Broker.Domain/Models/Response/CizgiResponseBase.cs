using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class CizgiResponseBase
    {
        public List<AvaibilityVehicleResponse> results { get; set; }
        public List<AvaibilityExtraResponse> packages { get; set; }
        public AvaibilityQueryResponse query { get; set; }
        public string search_id { get; set; }

        public class AuthLoginResponse
        {
            public string access_token { get; set; }
            public string issuer { get; set; }
            public string token_type { get; set; }
            public long expires_in { get; set; }
        }

        public class CommonResponse
        {
            public int status { get; set; }
            public string message { get; set; }

        }

        public class LocationResponse : CommonResponse
        {
            public List<Destination> destinations { get; set; }
        }
        public class Destination
        {
            public int destId { get; set; }
            public string destName { get; set; }
            public string destType { get; set; }
            public string airportCode { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string address { get; set; }
            public string cityName { get; set; }
        }

        public class VehicleResponse : CommonResponse
        {
            public List<Fleet> fleet { get; set; }
        }

        public class Fleet
        {
            public int carId { get; set; }
            public string carName { get; set; }
            public string fuel { get; set; }
            public string transmission { get; set; }
            public string type { get; set; }
            public string minLicenceAge { get; set; }
            public string minDriverAge { get; set; }
            public string seats { get; set; }
            public string baggage { get; set; }
            public int deposit { get; set; }
            public string deposit_currency { get; set; }
            public string SIPP { get; set; }
        }

        public class AvaibilityVehicleResponse
        {
            public int car_id { get; set; }
            public string sipp { get; set; }
            public string name { get; set; }
            public float? deposit { get; set; }
            public string currency { get; set; }
            public bool is_available { get; set; }
            public string per_day { get; set; }
            public string milage_limit { get; set; }
            public string for_duration { get; set; }
            public string oneway_fee { get; set; }
            public string driver_age { get; set; }
            public string licence_age { get; set; }
            public double alp { get; set; }
        }

        public class AvaibilityExtraResponse
        {
            public int id { get; set; }
            public string eqp { get; set; }
            public string equipment { get; set; }
            public int max_amount { get; set; }
            public int per_day { get; set; }
            public int for_duration { get; set; }
        }

        public class AvaibilityQueryResponse
        {
            public int duration { get; set; }
            public string pickup_date { get; set; }
            public string dropoff_date { get; set; }
            public int pickup_location { get; set; }
            public int dropoff_location { get; set; }
            public List<object> applied_codes { get; set; }
        }

        public class ExtraResponse : CommonResponse
        {
            public List<ExtraList> packages { get; set; }
        }

        public class ExtraList
        {
            public int id { get; set; }
            public string eqp { get; set; }
            public string equipment { get; set; }
            public int max_amount { get; set; }
        }

        public class PostReservationResponse : CommonResponse
        {
            public string reference_no { get; set; }
        }
    }
}
