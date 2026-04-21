using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.RecordGo.Requests
{
    public class RecordGoRequestBase
    {
        public class AvailableExtrasRequest
        {
            [JsonProperty("partnerUser")]
            public string partnerUser { get; set; }

            [JsonProperty("country")]
            public string country { get; set; }

            [JsonProperty("sellCode")]
            public string sellCode { get; set; }

            [JsonProperty("sellCodeVer")]
            public string sellCodeVer { get; set; }

            [JsonProperty("productVer")]
            public int productVer { get; set; }

            [JsonProperty("acrissCode")]
            public string acrissCode { get; set; }

            [JsonProperty("pickupBranch")]
            public int pickupBranch { get; set; }

            [JsonProperty("dropoffBranch")]
            public int dropoffBranch { get; set; }

            [JsonProperty("pickupLocation")]
            public int pickupLocation { get; set; }

            [JsonProperty("dropoffLocation")]
            public int dropoffLocation { get; set; }

            [JsonProperty("pickupDateTime")]
            public DateTime pickupDateTime { get; set; }

            [JsonProperty("dropoffDateTime")]
            public DateTime dropoffDateTime { get; set; }

            [JsonProperty("driverAge")]
            public int driverAge { get; set; }

            [JsonProperty("language")]
            public string language { get; set; }
        }
        public class ReservationCancelRequest
        {
            [JsonProperty("partnerUser")]
            public string partnerUser { get; set; }

            [JsonProperty("country")]
            public string country { get; set; }

            [JsonProperty("sellCodeVer")]
            public string sellCodeVer { get; set; }

            [JsonProperty("partnerBookingCode")]
            public string partnerBookingCode { get; set; }

            [JsonProperty("bookingAction")]
            public string bookingAction { get; set; }

            [JsonProperty("bookingStatus")]
            public string bookingStatus { get; set; }
        }
        public class ReservationRequest
        {
            [JsonProperty("partnerUser")]
            public string partnerUser { get; set; }

            [JsonProperty("country")]
            public string country { get; set; }

            [JsonProperty("sellCode")]
            public string sellCode { get; set; }

            [JsonProperty("sellCodeVer")]
            public string sellCodeVer { get; set; }

            [JsonProperty("pickupBranch")]
            public int pickupBranch { get; set; }

            [JsonProperty("dropoffBranch")]
            public int dropoffBranch { get; set; }

            [JsonProperty("pickupLocation")]
            public int pickupLocation { get; set; }

            [JsonProperty("dropoffLocation")]
            public int dropoffLocation { get; set; }

            [JsonProperty("partnerBookingCode")]
            public int partnerBookingCode { get; set; }

            [JsonProperty("bookingDate")]
            public DateTime bookingDate { get; set; }

            [JsonProperty("customer")]
            public Customer customer { get; set; }

            [JsonProperty("productId")]
            public string productId { get; set; }

            [JsonProperty("productVer")]
            public string productVer { get; set; }

            [JsonProperty("rateProdVer")]
            public string rateProdVer { get; set; }

            [JsonProperty("acrissCode")]
            public string acrissCode { get; set; }

            [JsonProperty("productAutomaticComplements")]
            public List<ProductAutomaticComplement> productAutomaticComplements { get; set; }

            [JsonProperty("productAssociatedComplements")]
            public List<ProductAssociatedComplement> productAssociatedComplements { get; set; }

            [JsonProperty("bookingTotalAmount")]
            public float bookingTotalAmount { get; set; }

            [JsonProperty("finalCustomerTotalAmount")]
            public float finalCustomerTotalAmount { get; set; }

            [JsonProperty("pickupDateTime")]
            public DateTime pickupDateTime { get; set; }

            [JsonProperty("dropoffDateTime")]
            public DateTime dropoffDateTime { get; set; }

            [JsonProperty("driverAge")]
            public int driverAge { get; set; }

            [JsonProperty("language")]
            public string language { get; set; }
            public class Customer
            {
                [JsonProperty("name")]
                public string name { get; set; }

                [JsonProperty("surname")]
                public string surname { get; set; }
            }

            public class ProductAssociatedComplement
            {
                [JsonProperty("complementId")]
                public int complementId { get; set; }

                [JsonProperty("complementVer")]
                public int complementVer { get; set; }

                [JsonProperty("complementUnits")]
                public int complementUnits { get; set; }

                [JsonProperty("rateComplVer")]
                public int rateComplVer { get; set; }

                [JsonProperty("priceTaxIncComplement")]
                public double priceTaxIncComplement { get; set; }

                [JsonProperty("finalPriceTaxIncComplement")]
                public double finalPriceTaxIncComplement { get; set; }
            }

            public class ProductAutomaticComplement
            {
                [JsonProperty("complementId")]
                public int complementId { get; set; }

                [JsonProperty("complementVer")]
                public int complementVer { get; set; }

                [JsonProperty("complementUnits")]
                public int complementUnits { get; set; }

                [JsonProperty("rateComplVer")]
                public int rateComplVer { get; set; }

                [JsonProperty("priceTaxIncComplement")]
                public double priceTaxIncComplement { get; set; }

                [JsonProperty("finalPriceTaxIncComplement")]
                public double finalPriceTaxIncComplement { get; set; }
            }
        }
        public class VehicleAvailabilityRequest
        {
            [JsonProperty("partnerUser")]
            public string partnerUser { get; set; }

            [JsonProperty("country")]
            public string country { get; set; }

            [JsonProperty("sellCode")]
            public string sellCode { get; set; }

            [JsonProperty("sellCodeVer")]
            public string sellCodeVer { get; set; }

            [JsonProperty("pickupBranch")]
            public int pickupBranch { get; set; }

            [JsonProperty("dropoffBranch")]
            public int dropoffBranch { get; set; }

            [JsonProperty("pickupLocation")]
            public int pickupLocation { get; set; }

            [JsonProperty("dropoffLocation")]
            public int dropoffLocation { get; set; }

            [JsonProperty("pickupDateTime")]
            public DateTime pickupDateTime { get; set; }

            [JsonProperty("dropoffDateTime")]
            public DateTime dropoffDateTime { get; set; }

            [JsonProperty("driverAge")]
            public int driverAge { get; set; }

            [JsonProperty("language")]
            public string language { get; set; }
        }
    }
}
