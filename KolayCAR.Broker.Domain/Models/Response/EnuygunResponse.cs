using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class EnuygunResponse
    {

        public class Login
        {
            public class Data
            {
                public string Token { get; set; }
                public string Expire { get; set; }
            }

            public class Root
            {
                public string Status { get; set; }
                public string Message { get; set; }
                public string UserMessage { get; set; }
                public Data Data { get; set; }

            }
        }

        public class Locations
        {
            public class Location
            {
                public string Name { get; set; }
                public string Slug { get; set; }
                public string City { get; set; }
                public string Country { get; set; }
                public string Latitude { get; set; }
                public string Longitude { get; set; }
            }

            public class Root
            {
                public string Status { get; set; }
                public string Message { get; set; }
                public string UserMessage { get; set; }
                public List<Location> Data { get; set; }
            }
        }

        public class Search
        {

            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
            public class Breakdown
            {
                public Raw raw { get; set; }
                public string currency { get; set; }
                public string type { get; set; }
                public double chargePrice { get; set; }
                public double officePrice { get; set; }
                public double totalPrice { get; set; }
            }

            public class Company
            {
                public string matchCode { get; set; }
                public string logoUri { get; set; }
                public string name { get; set; }
                public string slug { get; set; }
            }

            public class Data
            {
                public string requestId { get; set; }
                public List<Reservation> reservations { get; set; }
            }

            public class DropOffOffice
            {
                public string address { get; set; }
                public string phoneNumber { get; set; }
                public string email { get; set; }
                public bool boardingPass { get; set; }
                public string referenceId { get; set; }
                public object latitude { get; set; }
                public object longitude { get; set; }
            }

            public class PickUpOffice
            {
                public string address { get; set; }
                public string phoneNumber { get; set; }
                public string email { get; set; }
                public bool boardingPass { get; set; }
                public string referenceId { get; set; }
                public object latitude { get; set; }
                public object longitude { get; set; }
            }

            public class Price
            {
                public double chargePrice { get; set; }
                public double officePrice { get; set; }
                public double dailyPrice { get; set; }
                public double totalPrice { get; set; }
                public string currency { get; set; }
            }

            public class ProvisionPrice
            {
                public double price { get; set; }
                public string currency { get; set; }
            }

            public class Raw
            {
                public float price { get; set; }
                public float totalPrice { get; set; }
                public string currency { get; set; }
            }

            public class Reservation
            {
                public string referenceId { get; set; }
                public string status { get; set; }
                public int days { get; set; }
                public string provider { get; set; }
                public int limitedKm { get; set; }
                public string deliveryType { get; set; }
                public int dailyLimitedKm { get; set; }
                public int licenceYear { get; set; }
                public int driverAge { get; set; }
                public int freeCancellationHour { get; set; }
                public int maxCancellationHour { get; set; }
                public double cancellationPenalty { get; set; }
                public Price price { get; set; }
                public ProvisionPrice provisionPrice { get; set; }
                public Vehicle vehicle { get; set; }
                public Company company { get; set; }
                public PickUpOffice pickUpOffice { get; set; }
                public DropOffOffice dropOffOffice { get; set; }
                public List<Breakdown> breakdowns { get; set; }
            }

            public class Root
            {
                public string status { get; set; }
                public string message { get; set; }
                public string userMessage { get; set; }
                public Data data { get; set; }
            }

            public class Vehicle
            {
                public string brand { get; set; }
                public string fuel { get; set; }
                public string transmission { get; set; }
                public string imageUrl { get; set; }
                public string @class { get; set; }
                public int chair { get; set; }
                public string name { get; set; }
                public string matchCode { get; set; }
            }



        }

        public class Reservation
        {
            public class Cancel
            {
                public string status { get; set; }
                public string message { get; set; }
                public string userMessage { get; set; }
                public List<object> data { get; set; }
            }

            public class Data
            {
                public string orderId { get; set; }
                public string providerOrderId { get; set; }
            }

            public class Root
            {
                public string status { get; set; }
                public string message { get; set; }
                public string userMessage { get; set; }
                public Data data { get; set; }
            }




        }

        public class Extra
        {
            public class Data
            {
                public List<ExtraService> extraServices { get; set; }
            }

            public class ExtraService
            {
                public string type { get; set; }
                public string context { get; set; }
                public string name { get; set; }
                public int maxCount { get; set; }
                public double unitPrice { get; set; }
                public double chargePrice { get; set; }
                public double officePrice { get; set; }
                public double totalPrice { get; set; }
                public string currency { get; set; }
            }

            public class Root
            {
                public string status { get; set; }
                public string message { get; set; }
                public string userMessage { get; set; }
                public Data data { get; set; }
            }
        }


    }
}
