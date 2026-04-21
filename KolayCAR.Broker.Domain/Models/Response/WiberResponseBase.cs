using Newtonsoft.Json;
using System.Collections.Generic;
namespace KolayCAR.Broker.Domain.Models.Response
{
    public class WiberResponseBase
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
                [JsonProperty("code")]
                public int code { get; set; }

                [JsonProperty("name")]
                public string name { get; set; }

                [JsonProperty("address")]
                public string address { get; set; }

                [JsonProperty("phoneNumber")]
                public string phoneNumber { get; set; }
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
                [JsonProperty("Groups")]
                public string Groups { get; set; }

                [JsonProperty("SIPP")]
                public string SIPP { get; set; }

                [JsonProperty("Location")]
                public string Location { get; set; }

                [JsonProperty("ModelorSimilar")]
                public string ModelorSimilar { get; set; }

                [JsonProperty("Type")]
                public string Type { get; set; }

                [JsonProperty("People")]
                public string People { get; set; }

                [JsonProperty("Doors")]
                public string Doors { get; set; }

                [JsonProperty("GPS")]
                public string GPS { get; set; }

                [JsonProperty("Auto")]
                public string Auto { get; set; }

                [JsonProperty("AC")]
                public string AC { get; set; }

                [JsonProperty("Luggage")]
                public string Luggage { get; set; }

                [JsonProperty("Guaranted Model")]
                public string GuarantedModel { get; set; }

                [JsonProperty("Excessamount")]
                public float Excessamount { get; set; }

                [JsonProperty("UnlimitedMileage")]
                public int UnlimitedMileage { get; set; }
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
                public float PricePerDay { get; set; }

                [JsonProperty("maxPrice")]
                public float maxPrice { get; set; }

            }

            public class Root
            {
                [JsonProperty("extras")]
                public List<Extra> extras { get; set; }
            }





        }
        public class AvailabilityVehicles
        {
            public class Rate
            {
                [JsonProperty("a:CodRate")]
                public string CodRate { get; set; }

                [JsonProperty("a:PriceDay")]
                public string PriceDay { get; set; }

                [JsonProperty("a:PriceTot")]
                public string PriceTot { get; set; }

                public string groupName { get; set; }
            }
            public class ARates
            {
                [JsonProperty("a:Rate")]
                public object aRate { get; set; }
            }

            public class AVehicle
            {
                [JsonProperty("a:Disponibles")]
                public string aDisponibles { get; set; }

                [JsonProperty("a:Group")]
                public string aGroup { get; set; }

                [JsonProperty("a:Rates")]
                public ARates aRates { get; set; }

                public string Sipp { get; set; }
            }

            public class AVehicles
            {
                [JsonProperty("a:Vehicle")]
                public List<AVehicle> aVehicle { get; set; }
            }

            public class Root
            {
                [JsonProperty("s:Envelope")]
                public SEnvelope sEnvelope { get; set; }
            }

            public class SBody
            {
                [JsonProperty("SearchCarRQResponse")]
                public SearchCarRQResponse SearchCarRQResponse { get; set; }
            }

            public class SearchCarRQResponse
            {
                [JsonProperty("@xmlns")]
                public string xmlns { get; set; }

                [JsonProperty("SearchCarRQResult")]
                public SearchCarRQResult SearchCarRQResult { get; set; }
            }

            public class SearchCarRQResult
            {
                [JsonProperty("@xmlns:a")]
                public string xmlnsa { get; set; }

                [JsonProperty("@xmlns:i")]
                public string xmlnsi { get; set; }

                [JsonProperty("a:Days")]
                public string aDays { get; set; }

                [JsonProperty("a:ErrorCode")]
                public string aErrorCode { get; set; }

                [JsonProperty("a:ErrorMessage")]
                public object aErrorMessage { get; set; }

                [JsonProperty("a:ExecutionTime")]
                public string aExecutionTime { get; set; }

                [JsonProperty("a:Vehicles")]
                public AVehicles aVehicles { get; set; }
            }

            public class SEnvelope
            {
                [JsonProperty("@xmlns:s")]
                public string xmlnss { get; set; }

                [JsonProperty("s:Body")]
                public SBody sBody { get; set; }
            }


        }
        public class AvailabilityExtras
        {
            [JsonProperty("extras")]
            public List<Extras> extras { get; set; }
        }
        public class CreateReservation
        {
            public class ALink
            {
                [JsonProperty("@i:nil")]
                public string inil { get; set; }
            }

            public class MakeBookingRQResponse
            {
                [JsonProperty("@xmlns")]
                public string xmlns { get; set; }

                [JsonProperty("MakeBookingRQResult")]
                public MakeBookingRQResult MakeBookingRQResult { get; set; }
            }

            public class MakeBookingRQResult
            {
                [JsonProperty("@xmlns:a")]
                public string xmlnsa { get; set; }

                [JsonProperty("@xmlns:i")]
                public string xmlnsi { get; set; }

                [JsonProperty("a:ErrorCode")]
                public string aErrorCode { get; set; }

                [JsonProperty("a:ErrorMessage")]
                public string aErrorMessage { get; set; }

                [JsonProperty("a:Link")]
                public ALink aLink { get; set; }

                [JsonProperty("a:ReserveStatus")]
                public string aReserveStatus { get; set; }

                [JsonProperty("a:Voucher")]
                public string aVoucher { get; set; }
            }

            public class Root
            {
                [JsonProperty("s:Envelope")]
                public SEnvelope sEnvelope { get; set; }
            }

            public class SBody
            {
                [JsonProperty("MakeBookingRQResponse")]
                public MakeBookingRQResponse MakeBookingRQResponse { get; set; }
            }

            public class SEnvelope
            {
                [JsonProperty("@xmlns:s")]
                public string xmlnss { get; set; }

                [JsonProperty("s:Body")]
                public SBody sBody { get; set; }
            }


        }
        public class CancelReservation
        {
            public class AMessageStatus
            {
                [JsonProperty("@i:nil")]
                public string inil { get; set; }
            }

            public class CancelBookingRQResponse
            {
                [JsonProperty("@xmlns")]
                public string xmlns { get; set; }

                [JsonProperty("CancelBookingRQResult")]
                public CancelBookingRQResult CancelBookingRQResult { get; set; }
            }

            public class CancelBookingRQResult
            {
                [JsonProperty("@xmlns:a")]
                public string xmlnsa { get; set; }

                [JsonProperty("@xmlns:i")]
                public string xmlnsi { get; set; }

                [JsonProperty("a:ErrorCode")]
                public string aErrorCode { get; set; }

                [JsonProperty("a:ErrorMessage")]
                public string aErrorMessage { get; set; }

                [JsonProperty("a:MessageStatus")]
                public AMessageStatus aMessageStatus { get; set; }

                [JsonProperty("a:ReserveStatus")]
                public string aReserveStatus { get; set; }
            }

            public class Root
            {
                [JsonProperty("s:Envelope")]
                public SEnvelope sEnvelope { get; set; }
            }

            public class SBody
            {
                [JsonProperty("CancelBookingRQResponse")]
                public CancelBookingRQResponse CancelBookingRQResponse { get; set; }
            }

            public class SEnvelope
            {
                [JsonProperty("@xmlns:s")]
                public string xmlnss { get; set; }

                [JsonProperty("s:Body")]
                public SBody sBody { get; set; }
            }


        }
    }
}