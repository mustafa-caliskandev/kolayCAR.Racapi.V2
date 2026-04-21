using Newtonsoft.Json;
using System.Collections.Generic;
namespace KolayCAR.Broker.Domain.Models.Response
{
    public class ThermeonResponseBase
    {
        public class Location
        {
            public class Country
            {
                [JsonProperty("code")]
                public string code { get; set; }

                [JsonProperty("name")]
                public string name { get; set; }

                [JsonProperty("stations")]
                public List<Station> stations { get; set; }
            }

            public class Root
            {
                [JsonProperty("Countries")]
                public List<Country> Countries { get; set; }
            }

            public class Station
            {
                [JsonProperty("LocationId")]
                public int LocationId { get; set; }

                [JsonProperty("LocationCode")]
                public string LocationCode { get; set; }

                [JsonProperty("City name")]
                public string Cityname { get; set; }

                [JsonProperty("LocationName")]
                public string LocationName { get; set; }

                [JsonProperty("LocationType")]
                public string LocationType { get; set; }

                [JsonProperty("Address")]
                public string Address { get; set; }

                [JsonProperty("Latitude")]
                public string Latitude { get; set; }

                [JsonProperty("Longitude")]
                public string Longitude { get; set; }

                [JsonProperty("Telephone")]
                public string Telephone { get; set; }

                [JsonProperty("Emailaddress")]
                public string Emailaddress { get; set; }

                [JsonProperty("Arrival and return instructions for customers")]
                public string Arrivalandreturninstructionsforcustomers { get; set; }

                [JsonProperty("Monday")]
                public string Monday { get; set; }

                [JsonProperty("Tuesday")]
                public string Tuesday { get; set; }

                [JsonProperty("Wednesday")]
                public string Wednesday { get; set; }

                [JsonProperty("Thursday")]
                public string Thursday { get; set; }

                [JsonProperty("Friday")]
                public string Friday { get; set; }

                [JsonProperty("Saturday")]
                public string Saturday { get; set; }

                [JsonProperty("Sunday")]
                public string Sunday { get; set; }

                [JsonProperty("OOH Pick-up")]
                public string OOHPickup { get; set; }

                [JsonProperty("OOH Drop-off")]
                public string OOHDropoff { get; set; }

                [JsonProperty("Applicable Fee")]
                public string ApplicableFee { get; set; }

                [JsonProperty("Keybox yes/no")]
                public string Keyboxyesno { get; set; }

                [JsonProperty("Remarks")]
                public string Remarks { get; set; }
            }




        }
        public class Vehicles
        {
            public class Root
            {
                [JsonProperty("vehicles")]
                public List<Vehicle> vehicles { get; set; }
            }

            public class Vehicle
            {
                [JsonProperty("ACRISS")]
                public string ACRISS { get; set; }

                [JsonProperty("Doors")]
                public string Doors { get; set; }

                [JsonProperty("Passengers")]
                public string Passengers { get; set; }

                [JsonProperty("CDW and TP Excess")]
                public string CDWandTPExcess { get; set; }

                [JsonProperty("Make & Model Guaranteed")]
                public string MakeModelGuaranteed { get; set; }

                [JsonProperty("GPS Guaranteed")]
                public string GPSGuaranteed { get; set; }

                [JsonProperty("Fuel")]
                public string Fuel { get; set; }

                [JsonProperty("Transmission")]
                public string Transmission { get; set; }

                [JsonProperty("A/C")]
                public string AC { get; set; }

                [JsonProperty("Location Available")]
                public string LocationAvailable { get; set; }

                [JsonProperty("ModelNameDescription")]
                public string ModelNameDescription { get; set; }

                [JsonProperty("SupplierCode")]
                public string SupplierCode { get; set; }

                [JsonProperty("MinAge")]
                public string MinAge { get; set; }

                [JsonProperty("LuggageCapacity")]
                public string LuggageCapacity { get; set; }
            }




        }
        public class Extras
        {
            public class Extra
            {
                [JsonProperty("extraCode")]
                public string extraCode { get; set; }

                [JsonProperty("description")]
                public string description { get; set; }

                [JsonProperty("PricePerDay")]
                public double PricePerDay { get; set; }

                [JsonProperty("maxPrice")]
                public int maxPrice { get; set; }
            }

            public class Root
            {
                [JsonProperty("extras")]
                public List<Extra> extras { get; set; }
            }


        }
        public class AvailabilityVehicles
        {

            public class Distance
            {
                [JsonProperty("Included")]
                public string Included { get; set; }
            }

            public class DropCharge
            {
                [JsonProperty("@responsibility")]
                public string responsibility { get; set; }

                [JsonProperty("#text")]
                public string text { get; set; }
            }

            public class Rate
            {
                [JsonProperty("RateID")]
                public string RateID { get; set; }

                [JsonProperty("Class")]
                public string Class { get; set; }

                [JsonProperty("Availability")]
                public string Availability { get; set; }

                [JsonProperty("CurrencyCode")]
                public string CurrencyCode { get; set; }

                [JsonProperty("Estimate")]
                public string Estimate { get; set; }

                [JsonProperty("RateOnlyEstimate")]
                public string RateOnlyEstimate { get; set; }

                [JsonProperty("DropCharge")]
                public DropCharge DropCharge { get; set; }

                [JsonProperty("Distance")]
                public Distance Distance { get; set; }

                [JsonProperty("Liability")]
                public string Liability { get; set; }

                [JsonProperty("PrePaid")]
                public string PrePaid { get; set; }

                [JsonProperty("AlternateRateProduct")]
                public List<string> AlternateRateProduct { get; set; }
            }

            public class Response
            {
                [JsonProperty("@xmlns")]
                public string xmlns { get; set; }

                [JsonProperty("@regardingReferenceNumber")]
                public string regardingReferenceNumber { get; set; }

                [JsonProperty("@version")]
                public string version { get; set; }

                [JsonProperty("@webxg_id")]
                public string webxg_id { get; set; }

                [JsonProperty("ResRates")]
                public ResRates ResRates { get; set; }
            }

            public class ResRates
            {
                [JsonProperty("@success")]
                public string success { get; set; }

                [JsonProperty("Count")]
                public string Count { get; set; }

                [JsonProperty("Rate")]
                public List<Rate> Rate { get; set; }
            }

            public class Root
            {
                [JsonProperty("Response")]
                public Response Response { get; set; }
            }



        }
        public class CreateReservation
        {

            public class NewReservationResponse
            {
                [JsonProperty("@success")]
                public string success { get; set; }

                [JsonProperty("@reservationStatus")]
                public string reservationStatus { get; set; }

                [JsonProperty("@reservationNumber")]
                public string reservationNumber { get; set; }

                [JsonProperty("@reservationConfirmed")]
                public string reservationConfirmed { get; set; }
            }

            public class Response
            {
                [JsonProperty("@xmlns")]
                public string xmlns { get; set; }

                [JsonProperty("@regardingReferenceNumber")]
                public string regardingReferenceNumber { get; set; }

                [JsonProperty("@version")]
                public string version { get; set; }

                [JsonProperty("@webxg_id")]
                public string webxg_id { get; set; }

                [JsonProperty("NewReservationResponse")]
                public NewReservationResponse NewReservationResponse { get; set; }
            }

            public class Root
            {
                [JsonProperty("Response")]
                public Response Response { get; set; }
            }


        }
        public class CancelReservation
        {
            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
            public class CancelReservationResponse
            {
                [JsonProperty("@success")]
                public string success { get; set; }
            }

            public class Response
            {
                [JsonProperty("@xmlns")]
                public string xmlns { get; set; }

                [JsonProperty("@regardingReferenceNumber")]
                public string regardingReferenceNumber { get; set; }

                [JsonProperty("@version")]
                public string version { get; set; }

                [JsonProperty("@webxg_id")]
                public string webxg_id { get; set; }

                [JsonProperty("CancelReservationResponse")]
                public CancelReservationResponse CancelReservationResponse { get; set; }
            }

            public class Root
            {
                [JsonProperty("Response")]
                public Response Response { get; set; }
            }



        }
    }
}