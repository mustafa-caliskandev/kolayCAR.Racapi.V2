using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class EnuygunRequest
    {

        public class Login
        {
            public string username { get; set; }
            public string password { get; set; }
        }


        public class Search
        {
            public class Vehicles
            {
                public string pickUpDate { get; set; }
                public string pickUpTime { get; set; }
                public string dropOffDate { get; set; }
                public string dropOffTime { get; set; }
                public string pickUpLocation { get; set; }
                public string dropOffLocation { get; set; }
                public string currency { get; set; }
                public BrokerParameters brokerParameters { get; set; }
            }

            public class BrokerParameters
            {
                public int ratio { get; set; }
                public string contractType { get; set; }
            }
        }

        public class Reservation
        {
            public class Cancel
            {
                public string requestId { get; set; }
                public string referenceId { get; set; }
            }


            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
            public class Contact
            {
                public bool subscribe { get; set; }
                public string email { get; set; }
                public string phoneCountryCode { get; set; }
                public string phoneNumber { get; set; }
                public string firstName { get; set; }
                public string lastName { get; set; }
                public string birthDay { get; set; }
                public string citizenNumber { get; set; }
                public string flightNumber { get; set; }
                public string passportNumber { get; set; }
            }

            public class ExtraService
            {
                public string slug { get; set; }
                public string count { get; set; }
            }

            public class Invoice
            {
                public string type { get; set; }
                public string country { get; set; }
                public string city { get; set; }
                public string address { get; set; }
                public string taxNumber { get; set; }
                public string taxOffice { get; set; }
                public string postCode { get; set; }
                public string corporateName { get; set; }
                public bool isPersonalCompany { get; set; }
            }

            public class Root
            {
                public string requestId { get; set; }
                public string referenceId { get; set; }
                public Contact contact { get; set; }
                public Invoice invoice { get; set; }
                public List<ExtraService> extraServices { get; set; }
                public float? salePrice { get; set; }
                public string paymentType { get; set; }
            }



        }

        public class Extra
        {

            public class Extras
            {
                public string requestId { get; set; }
                public string referenceId { get; set; }
            }
        }

    }
}
