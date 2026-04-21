namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class TurmobilRequestBase
    {
        public class AuthLoginRequest
        {
            public string user { get; set; }
            public string password { get; set; }
        }
        //public class AvailabilityRequest
        //{
        //    public string pickup_date { get; set; }
        //    public int pickup_location { get; set; }
        //    public string dropoff_date { get; set; }
        //    public int dropoff_location { get; set; }
        //    public List<object> rate_codes { get; set; }
        //}

        //public class PostReservationRequest : AvailabilityRequest
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
        //    public Dictionary<string, int> packages { get; set; }
        //    public string search_id { get; set; }
        //}

        public class Extra
        {
            public string deliveryDate { get; set; }
            public string returnDate { get; set; }
            public string groupType { get; set; }
            public string lang { get; set; }
            public string curr { get; set; }
        }

        //public class CancelReservationRequest
        //{
        //    public string reference_no { get; set; }
        //    public string reason { get; set; }
        //}

        public class PostReservationRequest
        {
            public string customerName { get; set; }
            public string taxNo { get; set; }
            public string deliveryDate { get; set; }
            public string returnDate { get; set; }
            public string vehicleTypeId { get; set; }
            public string deliveryLocationId { get; set; }
            public string returnLocationId { get; set; }
            public string damageInsuranceId { get; set; }
            public string additionalServices { get; set; }
            public string customerPhone { get; set; }
            public bool isPaid { get; set; }
            public float? paidAmount { get; set; }
            public bool fullCredit { get; set; }
            public string curr { get; set; }
        }

        public class ReservationAdditionalProduct
        {
            public string id { get; set; }
        }

        public class PostCancelReservationRequest
        {
            public string uuid { get; set; }
        }
    }
}
