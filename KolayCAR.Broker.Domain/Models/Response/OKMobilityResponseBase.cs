using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class OKMobilityResponseBase
    {
        public class Location
        {
            public class GetStationsResult
            {
                public List<RentalStation> RentalStation { get; set; }
            }

            public class GetStationsResultResponse
            {
                public GetStationsResult getStationsResult { get; set; }
            }

            public class RentalStation
            {
                [JsonProperty("@rowNumber")]
                public string rowNumber { get; set; }
                public string StationID { get; set; }
                public string Station { get; set; }
                public string Address { get; set; }
                public string Zone { get; set; }
                public string StationType { get; set; }
                public string CountryID { get; set; }
                public string City { get; set; }
                public string Zipcode { get; set; }
                public string Phone { get; set; }
                public string Fax { get; set; }
                public string Latitude { get; set; }
                public string Longitude { get; set; }
                public string ActivateOutTime { get; set; }
                public string AfterHourpick { get; set; }
                public string AfterHourdrop { get; set; }
                public string HourStartMon { get; set; }
                public string HourStartSat { get; set; }
                public string HourStartSun { get; set; }
                public string HourStartHol { get; set; }
                public string HourOpenMon { get; set; }
                public string HourOpenSat { get; set; }
                public string HourOpenSun { get; set; }
                public string HourOpenHol { get; set; }
                public string LunchStartMon { get; set; }
                public string LunchStartSat { get; set; }
                public string LunchStartSun { get; set; }
                public string LunchStartHol { get; set; }
                public string LunchEndMon { get; set; }
                public string LunchEndSat { get; set; }
                public string LunchEndSun { get; set; }
                public string LunchEndHol { get; set; }
                public string HourCloseMon { get; set; }
                public string HourCloseSat { get; set; }
                public string HourCloseSun { get; set; }
                public string HourCloseHol { get; set; }
                public string HourEndMon { get; set; }
                public string HourEndSat { get; set; }
                public string HourEndSun { get; set; }
                public string HourEndHol { get; set; }
                public string HourStartTue { get; set; }
                public string HourStartWed { get; set; }
                public string HourStartThu { get; set; }
                public string HourStartFri { get; set; }
                public string HourOpenTue { get; set; }
                public string HourOpenWed { get; set; }
                public string HourOpenThu { get; set; }
                public string HourOpenFri { get; set; }
                public string LunchStartTue { get; set; }
                public string LunchStartWed { get; set; }
                public string LunchStartThu { get; set; }
                public string LunchStartFri { get; set; }
                public string LunchEndTue { get; set; }
                public string LunchEndWed { get; set; }
                public string LunchEndThu { get; set; }
                public string LunchEndFri { get; set; }
                public string HourCloseTue { get; set; }
                public string HourCloseWed { get; set; }
                public string HourCloseThu { get; set; }
                public string HourCloseFri { get; set; }
                public string HourEndTue { get; set; }
                public string HourEndWed { get; set; }
                public string HourEndThu { get; set; }
                public string HourEndFri { get; set; }
                public string toEngine { get; set; }
                public string open24h { get; set; }
            }

            public class Root
            {
                [JsonProperty("soap:Envelope")]
                public SoapEnvelope soapEnvelope { get; set; }
            }

            public class SoapBody
            {
                public GetStationsResultResponse getStationsResultResponse { get; set; }
            }

            public class SoapEnvelope
            {
                [JsonProperty("@xmlns:soap")]
                public string xmlnssoap { get; set; }

                [JsonProperty("@xmlns:xsi")]
                public string xmlnsxsi { get; set; }

                [JsonProperty("@xmlns:xsd")]
                public string xmlnsxsd { get; set; }

                [JsonProperty("soap:Body")]
                public SoapBody soapBody { get; set; }
            }
        }
        public class CarCategory
        {
            public class GetGroupsResult
            {
                public List<Group> Group { get; set; }
            }

            public class GetGroupsResultResponse
            {
                public GetGroupsResult getGroupsResult { get; set; }
            }

            public class Group
            {
                [JsonProperty("@rowNumber")]
                public string rowNumber { get; set; }
                public string GroupCode { get; set; }
                public string Description { get; set; }
                public string sipp { get; set; }
            }

            public class Root
            {
                [JsonProperty("soap:Envelope")]
                public SoapEnvelope soapEnvelope { get; set; }
            }

            public class SoapBody
            {
                public GetGroupsResultResponse getGroupsResultResponse { get; set; }
            }

            public class SoapEnvelope
            {
                [JsonProperty("@xmlns:soap")]
                public string xmlnssoap { get; set; }

                [JsonProperty("@xmlns:xsi")]
                public string xmlnsxsi { get; set; }

                [JsonProperty("@xmlns:xsd")]
                public string xmlnsxsd { get; set; }

                [JsonProperty("soap:Body")]
                public SoapBody soapBody { get; set; }
            }


        }
        public class CarDetails
        {
            public class Root
            {
                [JsonProperty("Country")]
                public string Country { get; set; }

                [JsonProperty("VehicleGroups")]
                public List<VehicleGroup> VehicleGroups { get; set; }

                [JsonProperty("CountryID")]
                public string CountryID { get; set; }
            }

            public class VehicleGroup
            {
                [JsonProperty("type")]
                public string type { get; set; }

                [JsonProperty("code")]
                public string code { get; set; }

                [JsonProperty("classification")]
                public string classification { get; set; }

                [JsonProperty("doors")]
                public int doors { get; set; }

                [JsonProperty("seats")]
                public object seats { get; set; }

                [JsonProperty("bags")]
                public int bags { get; set; }

                [JsonProperty("model")]
                public string model { get; set; }
            }

        }

        public class Extra
        {
            [JsonProperty("groupID")]
            public string groupID { get; set; }

            [JsonProperty("extraID")]
            public string extraID { get; set; }

            [JsonProperty("extra")]
            public string extra { get; set; }

            [JsonProperty("description")]
            public string description { get; set; }

            [JsonProperty("value")]
            public string value { get; set; }

            [JsonProperty("valueWithTax")]
            public string valueWithTax { get; set; }

            [JsonProperty("taxRate")]
            public string taxRate { get; set; }

            [JsonProperty("extra_Included")]
            public string extra_Included { get; set; }

            [JsonProperty("extra_Required")]
            public string extra_Required { get; set; }

            [JsonProperty("extra_Accepted")]
            public string extra_Accepted { get; set; }

            [JsonProperty("accept_quantity")]
            public string accept_quantity { get; set; }

            [JsonProperty("insurance")]
            public string insurance { get; set; }

            [JsonProperty("excess")]
            public string excess { get; set; }

            [JsonProperty("pricePerContract")]
            public string pricePerContract { get; set; }

            [JsonProperty("percentualDiscount")]
            public string percentualDiscount { get; set; }
        }

        public class Extras
        {
            [JsonProperty("extras")]
            public List<Extra> extras { get; set; }
        }
        public class Availability
        {
            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
            public class AllExtra
            {
                [JsonProperty("@rowNumber")]
                public string rowNumber { get; set; }

                [JsonProperty("groupID")]
                public string groupID { get; set; }

                [JsonProperty("extraID")]
                public string extraID { get; set; }

                [JsonProperty("extra")]
                public string extra { get; set; }

                [JsonProperty("description")]
                public string description { get; set; }

                [JsonProperty("value")]
                public string value { get; set; }

                [JsonProperty("valueWithTax")]
                public string valueWithTax { get; set; }

                [JsonProperty("taxRate")]
                public string taxRate { get; set; }

                [JsonProperty("extra_Included")]
                public string extra_Included { get; set; }

                [JsonProperty("extra_Required")]
                public string extra_Required { get; set; }

                [JsonProperty("extra_Accepted")]
                public string extra_Accepted { get; set; }

                [JsonProperty("accept_quantity")]
                public string accept_quantity { get; set; }

                [JsonProperty("insurance")]
                public string insurance { get; set; }

                [JsonProperty("excess")]
                public string excess { get; set; }

                [JsonProperty("pricePerContract")]
                public string pricePerContract { get; set; }

                [JsonProperty("percentualDiscount")]
                public string percentualDiscount { get; set; }
            }

            public class AllExtras
            {
                [JsonProperty("@count")]
                public string count { get; set; }

                [JsonProperty("allExtra")]
                public List<AllExtra> allExtra { get; set; }
            }

            public class GetMultiplePrice
            {
                [JsonProperty("@rowNumber")]
                public string rowNumber { get; set; }

                [JsonProperty("stationID")]
                public string stationID { get; set; }

                [JsonProperty("Station")]
                public string Station { get; set; }

                [JsonProperty("weekDayOpen")]
                public string weekDayOpen { get; set; }

                [JsonProperty("weekDayClose")]
                public string weekDayClose { get; set; }

                [JsonProperty("supplier_Code")]
                public string supplier_Code { get; set; }

                [JsonProperty("GroupID")]
                public string GroupID { get; set; }

                [JsonProperty("Group_Name")]
                public string Group_Name { get; set; }

                [JsonProperty("SIPP")]
                public string SIPP { get; set; }

                [JsonProperty("imageURL")]
                public string imageURL { get; set; }

                [JsonProperty("rateCode")]
                public string rateCode { get; set; }

                [JsonProperty("token")]
                public string token { get; set; }

                [JsonProperty("dynamicRate")]
                public string dynamicRate { get; set; }

                [JsonProperty("nrDays")]
                public string nrDays { get; set; }

                [JsonProperty("dayValue")]
                public string dayValue { get; set; }

                [JsonProperty("totalDayValueWithTax")]
                public string totalDayValueWithTax { get; set; }

                [JsonProperty("kmsValue")]
                public string kmsValue { get; set; }

                [JsonProperty("kmsIncluded")]
                public string kmsIncluded { get; set; }

                [JsonProperty("upsellingToGroup")]
                public string upsellingToGroup { get; set; }

                [JsonProperty("previewValue")]
                public string previewValue { get; set; }

                [JsonProperty("valueWithoutTax")]
                public string valueWithoutTax { get; set; }

                [JsonProperty("taxRate")]
                public string taxRate { get; set; }

                [JsonProperty("otherTaxValue")]
                public string otherTaxValue { get; set; }

                [JsonProperty("taxValue")]
                public string taxValue { get; set; }

                [JsonProperty("previewValueWithoutExtras")]
                public string previewValueWithoutExtras { get; set; }

                [JsonProperty("previewValueWithoutExtrasWithoutTax")]
                public string previewValueWithoutExtrasWithoutTax { get; set; }

                [JsonProperty("extrasIncluded")]
                public string extrasIncluded { get; set; }

                [JsonProperty("extrasRequired")]
                public string extrasRequired { get; set; }

                [JsonProperty("extrasAccepted")]
                public string extrasAccepted { get; set; }

                [JsonProperty("extrasRequested")]
                public string extrasRequested { get; set; }

                [JsonProperty("extrasAvailable")]
                public string extrasAvailable { get; set; }

                [JsonProperty("dayValueWithDiscount")]
                public string dayValueWithDiscount { get; set; }

                [JsonProperty("dayValueWithDiscountWithTax")]
                public string dayValueWithDiscountWithTax { get; set; }

                [JsonProperty("previewValueWithDiscount")]
                public string previewValueWithDiscount { get; set; }

                [JsonProperty("valueWithDiscountWithoutTax")]
                public string valueWithDiscountWithoutTax { get; set; }

                [JsonProperty("percentualDiscount")]
                public string percentualDiscount { get; set; }

                [JsonProperty("prepaidRate")]
                public string prepaidRate { get; set; }

                [JsonProperty("allExtras")]
                public AllExtras allExtras { get; set; }
            }

            public class GetMultiplePricesResult
            {
                [JsonProperty("getMultiplePrice")]
                public List<GetMultiplePrice> getMultiplePrice { get; set; }
            }

            public class GetMultiplePricesResultResponse
            {
                [JsonProperty("getMultiplePricesResult")]
                public GetMultiplePricesResult getMultiplePricesResult { get; set; }
            }

            public class Root
            {
                [JsonProperty("soap:Envelope")]
                public SoapEnvelope soapEnvelope { get; set; }
            }

            public class SoapBody
            {
                [JsonProperty("getMultiplePricesResultResponse")]
                public GetMultiplePricesResultResponse getMultiplePricesResultResponse { get; set; }
            }

            public class SoapEnvelope
            {
                [JsonProperty("@xmlns:soap")]
                public string xmlnssoap { get; set; }

                [JsonProperty("@xmlns:xsi")]
                public string xmlnsxsi { get; set; }

                [JsonProperty("@xmlns:xsd")]
                public string xmlnsxsd { get; set; }

                [JsonProperty("soap:Body")]
                public SoapBody soapBody { get; set; }
            }




        }

        public class Reservation
        {
            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
            public class CreateReservationResult
            {
                [JsonProperty("Reservation_Nr")]
                public string Reservation_Nr { get; set; }

                [JsonProperty("Reservation_Value")]
                public string Reservation_Value { get; set; }

                [JsonProperty("Reservation_Value_Invoice")]
                public string Reservation_Value_Invoice { get; set; }

                [JsonProperty("Reservation_Value_Cashier")]
                public string Reservation_Value_Cashier { get; set; }

                [JsonProperty("Status")]
                public string Status { get; set; }
            }

            public class CreateReservationResultResponse
            {
                [JsonProperty("createReservationResult")]
                public CreateReservationResult createReservationResult { get; set; }
            }

            public class Root
            {
                [JsonProperty("soap:Envelope")]
                public SoapEnvelope soapEnvelope { get; set; }
            }

            public class SoapBody
            {
                [JsonProperty("createReservationResultResponse")]
                public CreateReservationResultResponse createReservationResultResponse { get; set; }
            }

            public class SoapEnvelope
            {
                [JsonProperty("@xmlns:soap")]
                public string xmlnssoap { get; set; }

                [JsonProperty("@xmlns:xsi")]
                public string xmlnsxsi { get; set; }

                [JsonProperty("@xmlns:xsd")]
                public string xmlnsxsd { get; set; }

                [JsonProperty("soap:Body")]
                public SoapBody soapBody { get; set; }
            }


        }

        public class CancelReservation
        {
            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
            public class CreateReservationResult
            {
                [JsonProperty("Reservation_Nr")]
                public string Reservation_Nr { get; set; }

                [JsonProperty("Reservation_Value")]
                public string Reservation_Value { get; set; }

                [JsonProperty("Reservation_Value_Invoice")]
                public string Reservation_Value_Invoice { get; set; }

                [JsonProperty("Reservation_Value_Cashier")]
                public string Reservation_Value_Cashier { get; set; }

                [JsonProperty("Status")]
                public string Status { get; set; }
            }

            public class CreateReservationResultResponse
            {
                [JsonProperty("createReservationResult")]
                public CreateReservationResult createReservationResult { get; set; }
            }

            public class Root
            {
                [JsonProperty("soap:Envelope")]
                public SoapEnvelope soapEnvelope { get; set; }
            }

            public class SoapBody
            {
                [JsonProperty("createReservationResultResponse")]
                public CreateReservationResultResponse createReservationResultResponse { get; set; }
            }

            public class SoapEnvelope
            {
                [JsonProperty("@xmlns:soap")]
                public string xmlnssoap { get; set; }

                [JsonProperty("@xmlns:xsi")]
                public string xmlnsxsi { get; set; }

                [JsonProperty("@xmlns:xsd")]
                public string xmlnsxsd { get; set; }

                [JsonProperty("soap:Body")]
                public SoapBody soapBody { get; set; }
            }


        }
    }

}