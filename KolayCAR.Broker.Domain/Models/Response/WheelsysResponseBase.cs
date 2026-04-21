using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class WheelsysResponseBase
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class OperationHours
        {
            public List<WorkHour> WorkHour { get; set; }
        }

        public class PickupInstructions
        {
            public string PickupInfo { get; set; }
        }
        public class Rates
        {
            public List<Category> category { get; set; }
        }
        public class Category
        {
            public Options options { get; set; }
            [JsonProperty("@availability")]
            public string availability { get; set; }
            [JsonProperty("@code")]
            public string code { get; set; }
            [JsonProperty("@model")]
            public string model { get; set; }
            [JsonProperty("@totalrate")]
            public string totalrate { get; set; }
            [JsonProperty("@baserate")]
            public string baserate { get; set; }
            [JsonProperty("@onewaycharge")]
            public string onewaycharge { get; set; }
            [JsonProperty("@outofhours")]
            public string outofhours { get; set; }
            [JsonProperty("@outofoffice")]
            public string outofoffice { get; set; }
            [JsonProperty("@excess")]
            public string excess { get; set; }
            [JsonProperty("@excessapplies")]
            public string excessapplies { get; set; }
            [JsonProperty("@klmincluded")]
            public string klmincluded { get; set; }
            [JsonProperty("@unlimited")]
            public string unlimited { get; set; }
            [JsonProperty("@addklmrate")]
            public string addklmrate { get; set; }
            [JsonProperty("@provision")]
            public string provision { get; set; }
        }
        public class Options
        {
            public List<Option> option { get; set; }
        }
        public class Pricequote
        {
            public Rates rates { get; set; }
            [JsonProperty("@id")]
            public string id { get; set; }
            [JsonProperty("@validto")]
            public string validto { get; set; }
            [JsonProperty("@duration")]
            public string duration { get; set; }
            [JsonProperty("@taxinclusive")]
            public string taxinclusive { get; set; }
            [JsonProperty("@currency")]
            public string currency { get; set; }
            [JsonProperty("@timetaken")]
            public string timetaken { get; set; }
            [JsonProperty("@graceperiod")]
            public string graceperiod { get; set; }
            public List<Errors> errors { get; set; }
        }
        public class Errors
        {
            public Error error { get; set; }
        }

        public class Error
        {
            [JsonProperty("@code")]
            public string code { get; set; }
            public string text { get; set; }
        }
        public class Response
        {
            [JsonProperty("@xmlns:xsd")]
            public string xmlnsxsd { get; set; }

            [JsonProperty("@xmlns:xsi")]
            public string xmlnsxsi { get; set; }
            public List<Station> station { get; set; }
            public string ImgRoot { get; set; }
            public List<Cargroup> cargroup { get; set; }
            public List<Option> option { get; set; }
            public Reservation reservation { get; set; }
        }

        public class Root
        {
            [JsonProperty("?xml")]
            public Xml xml { get; set; }
            public Response response { get; set; }
            public Pricequote pricequote { get; set; }

        }

        public class Station
        {
            [JsonProperty("@code")]
            public string code { get; set; }

            [JsonProperty("@name")]
            public string name { get; set; }

            [JsonProperty("@lat")]
            public string lat { get; set; }

            [JsonProperty("@long")]
            public string @long { get; set; }

            [JsonProperty("@icon")]
            public string icon { get; set; }

            [JsonProperty("@country")]
            public string country { get; set; }
            public StationInformation StationInformation { get; set; }
        }

        public class StationInformation
        {
            [JsonProperty("@StationType")]
            public string StationType { get; set; }

            [JsonProperty("@Tax1Percent")]
            public string Tax1Percent { get; set; }

            [JsonProperty("@Tax1Name")]
            public string Tax1Name { get; set; }

            [JsonProperty("@Address")]
            public string Address { get; set; }

            [JsonProperty("@ZipCode")]
            public string ZipCode { get; set; }

            [JsonProperty("@City")]
            public string City { get; set; }

            [JsonProperty("@Phone")]
            public string Phone { get; set; }
            public PickupInstructions PickupInstructions { get; set; }
            public OperationHours OperationHours { get; set; }
        }

        public class WorkHour
        {
            [JsonProperty("@Day")]
            public string Day { get; set; }

            [JsonProperty("@Closed")]
            public string Closed { get; set; }

            [JsonProperty("@OpensAt")]
            public string OpensAt { get; set; }

            [JsonProperty("@ClosesAt")]
            public string ClosesAt { get; set; }

            [JsonProperty("@MidBreakClose")]
            public string MidBreakClose { get; set; }

            [JsonProperty("@MidBreakOpen")]
            public string MidBreakOpen { get; set; }
        }

        public class Xml
        {
            [JsonProperty("@version")]
            public string version { get; set; }

            [JsonProperty("@encoding")]
            public string encoding { get; set; }
        }
        public class Cargroup
        {
            [JsonProperty("@code")]
            public string code { get; set; }

            [JsonProperty("@model")]
            public string model { get; set; }

            [JsonProperty("@imageurl")]
            public string imageurl { get; set; }

            [JsonProperty("@sippcode")]
            public string sippcode { get; set; }

            [JsonProperty("@pax")]
            public string pax { get; set; }

            [JsonProperty("@bags")]
            public string bags { get; set; }

            [JsonProperty("@doors")]
            public string doors { get; set; }

            [JsonProperty("@suitcases")]
            public string suitcases { get; set; }

            [JsonProperty("@cat")]
            public string cat { get; set; }
        }
        public class Option
        {
            [JsonProperty("@code")]
            public string code { get; set; }
            [JsonProperty("@name")]
            public string name { get; set; }
            [JsonProperty("@quant")]
            public string quant { get; set; }
            [JsonProperty("@rate")]
            public string rate { get; set; }
            [JsonProperty("@firstfree")]
            public string firstfree { get; set; }

        }
        public class Reservation
        {
            [JsonProperty("@irn")]
            public string irn { get; set; }
            [JsonProperty("@status")]
            public string status { get; set; }
            [JsonProperty("@refno")]
            public string refno { get; set; }
        }
    }
}
