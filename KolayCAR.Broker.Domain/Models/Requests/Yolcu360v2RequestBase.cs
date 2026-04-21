using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class Yolcu360v2RequestBase
    {
        public class Yolcu360v2AuthRequest
        {
            public string key { get; set; }
            public string secret { get; set; }
        }
        public class CheckLocation
        {
            public float lat { get; set; }
            public float lon { get; set; }
        }

        public class Commission
        {
            public string type { get; set; }
            public float percentage { get; set; }
        }

        public class Yolcu360v2SearchRequest
        {
            public string checkInDateTime { get; set; }
            public string checkOutDateTime { get; set; }
            public string age { get; set; }
            public string country { get; set; }
            public string paymentType { get; set; }
            public CheckLocation checkInLocation { get; set; }
            public CheckLocation checkOutLocation { get; set; }
            public Commission commission { get; set; }
            public bool fullCredit { get; set; }
        }
        public class Yolcu360v2PostReservationRequest
        {
            public string paymentType { get; set; }
            public string searchID { get; set; }
            public string code { get; set; }
            public List<ExtraProduct> extraProducts { get; set; }
            public Passenger passenger { get; set; }
            public Billing billing { get; set; }
            public bool isFullCredit { get; set; }
            public bool isLimitedCredit { get; set; }
            public string trackingID { get; set; }
        }
        public class Yolcu360v2PostPayRequest
        {
            public string orderID { get; set; }
            public string paymentType { get; set; }
        }
        public class Billing
        {
            //public string label { get; set; }
            public string type { get; set; }
            public string countryName { get; set; }
            public string countryCode { get; set; }
            public string firstName { get; set; }
            //public string lastName { get; set; }
            //public string email { get; set; }
            //public long phone { get; set; }
            //public string taxDivision { get; set; }
            //public string taxIdentifier { get; set; }
            //public int zipCode { get; set; }
            public string adm1 { get; set; }
            //public string adm2 { get; set; }
            //public string line { get; set; }
        }

        public class ExtraProduct
        {
            public int quantity { get; set; }
            public string code { get; set; }
        }

        public class Passenger
        {
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string email { get; set; }
            public string nationality { get; set; }
            public string phone { get; set; }
            public string? identityNumber { get; set; }
            public string? passportNo { get; set; }
            public string birthDate { get; set; }
        }
    }
}
