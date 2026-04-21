using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class CircularRequestBase
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

        //public class PostReservationRequest:AvailabilityRequest
        //{
        //    public string reference_no { get; set; }
        //    public int reference_id { get; set; }
        //    public string name { get; set; }
        //    public string surname { get; set; }
        //    public string email { get; set; }
        //    public string phone { get; set; }
        //    public string notes { get; set; }
        //    public string birth_date { get; set; }
        //    public string passport_no { get; set; }
        //    public string citizenship_no { get; set; }
        //    public string driver2_name { get; set; }
        //    public string driver2_surname { get; set; }
        //    public string driver2_email { get; set; }
        //    public string driver2_phone { get; set; }
        //    public string driver2_passport_no { get; set; }
        //    public string driver2_citizenship_no { get; set; }
        //    public int car_id { get; set; }
        //    public string flight_number { get; set; }
        //    public float payment_made { get; set; }
        //    public Dictionary<string,int> packages { get; set; }
        //    public string search_id { get; set; }
        //}

        public class ExtraList
        {
            public string value { get; set; }
        }

        //public class CancelReservationRequest
        //{
        //    public string reference_no { get; set; }
        //    public string reason { get; set; }
        //}

        public class PostCancelReservationRequestBody
        {
            public string referralno { get; set; }
            public string info { get; set; }
            public string id { get; set; }
        }

        public class PostReservationRequestBody
        {
            public string id { get; set; }
            public string currencycode { get; set; }
            public string reserveddate { get; set; }
            public string startdate { get; set; }
            public string starttime { get; set; }
            public string expectedenddate { get; set; }
            public string expectedendtime { get; set; }
            public string cargroup { get; set; }
            public string driver { get; set; }
            public string paytype { get; set; }
            public string pricing { get; set; }//"daily" => özel fiyatlandırma yok ise bu değeri alacak. "special" => Özel fiyat varsa bu değeri alacak
            public float? specialprice { get; set; } = null;
            public float dailyprice { get; set; }
            public string outlocation { get; set; }
            public string returnlocation { get; set; }
            public int referralagent { get; set; }
            public string referralno { get; set; }
            public float prepayment { get; set; }
            public string flightarrivalnumber { get; set; }
            public float dropprice { get; set; }
            public List<CircularRequestExtra> extra { get; set; }
        }

        public class CircularRequestExtra
        {
            public int id { get; set; }
            public float value { get; set; }
            public int include { get; set; }
        }
    }
}
