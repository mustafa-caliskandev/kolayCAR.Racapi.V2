using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class EganisRequestBase
    {
        public class AuthLoginRequest
        {
            public string key { get; set; }
            public string userName { get; set; }
            public string password { get; set; }
        }

        public class VehicleRequest
        {
            public int pickupLocationId { get; set; }
            public int dropOffLocationId { get; set; }
            public int pickupDay { get; set; }
            public int pickupMonth { get; set; }
            public int pickupYear { get; set; }
            public int pickupHour { get; set; }
            public int pickupMin { get; set; }
            public int dropOffDay { get; set; }
            public int dropOffMonth { get; set; }
            public int dropOffYear { get; set; }
            public int dropOffHour { get; set; }
            public int dropOffMin { get; set; }
            public string currencyCode { get; set; }
        }

        public class ReservationRequest
        {
            public class Extra
            {
                public string code { get; set; }
                public int quantity { get; set; }
                public float fee { get; set; }
            }

            public class Root
            {
                public int pickupLocationId { get; set; }
                public int dropOffLocationId { get; set; }
                public int pickupDay { get; set; }
                public int pickupMonth { get; set; }
                public int pickupYear { get; set; }
                public int pickupHour { get; set; }
                public int pickupMin { get; set; }
                public int dropOffDay { get; set; }
                public int dropOffMonth { get; set; }
                public int dropOffYear { get; set; }
                public int dropOffHour { get; set; }
                public int dropOffMin { get; set; }
                public string currencyCode { get; set; }
                public int vehGroupId { get; set; }
                public string sippCode { get; set; }
                public string passportId { get; set; }
                public string identityNr { get; set; }
                public string name { get; set; }
                public string surname { get; set; }
                public string birthDate { get; set; }
                public string driverLicenseDate { get; set; }
                public string phoneNr { get; set; }
                public string eMail { get; set; }
                public string address { get; set; }
                public string country { get; set; }
                public string city { get; set; }
                public string town { get; set; }
                public List<Extra> extras { get; set; }
                public string flightNr { get; set; }
                public string assurancePackageCode { get; set; }
                public int assurancePackageFee { get; set; }
                public float rentFee { get; set; }
                public float dropFee { get; set; }
                public float provisionFee { get; set; }
                public float paymentAmount { get; set; }
                public string refReservationId { get; set; }
            }


        }

        public class ReservationCancelRequest
        {

            public int reservationId { get; set; }
            public string cancelReason { get; set; }
            

        }
    }
}
