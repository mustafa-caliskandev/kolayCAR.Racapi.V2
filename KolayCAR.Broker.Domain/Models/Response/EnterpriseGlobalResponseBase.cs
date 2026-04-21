using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class EnterpriseGlobalResponseBase
    {
        public class AuthLoginResponse
        {
            public string Token { get; set; }
        }
        public class Locations
        {
            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
            public class Address
            {
                [JsonProperty("AddressLine")]
                public string AddressLine { get; set; }

                [JsonProperty("CityName")]
                public string CityName { get; set; }

                [JsonProperty("PostalCode")]
                public string PostalCode { get; set; }

                [JsonProperty("StateProv")]
                public StateProv StateProv { get; set; }

                [JsonProperty("CountryName")]
                public CountryName CountryName { get; set; }
            }

            public class CountryName
            {
                [JsonProperty("@Code")]
                public string Code { get; set; }
            }

            public class EnvBody
            {
                [JsonProperty("OTA_VehLocSearchRS")]
                public OTAVehLocSearchRS OTA_VehLocSearchRS { get; set; }
            }

            public class EnvEnvelope
            {
                [JsonProperty("@xmlns:env")]
                public string xmlnsenv { get; set; }

                [JsonProperty("@xmlns:xsd")]
                public string xmlnsxsd { get; set; }

                [JsonProperty("@xmlns:xsi")]
                public string xmlnsxsi { get; set; }

                [JsonProperty("env:Body")]
                public EnvBody envBody { get; set; }
            }

            public class LocationDetail
            {
                [JsonProperty("@AtAirport")]
                public string AtAirport { get; set; }

                [JsonProperty("@Code")]
                public string Code { get; set; }

                [JsonProperty("@Name")]
                public string Name { get; set; }

                [JsonProperty("Address")]
                public Address Address { get; set; }

                [JsonProperty("Telephone")]
                public List<Telephone> Telephone { get; set; }
            }

            public class OTAVehLocSearchRS
            {
                [JsonProperty("@TimeStamp")]
                public DateTime TimeStamp { get; set; }

                [JsonProperty("@TransactionIdentifier")]
                public string TransactionIdentifier { get; set; }

                [JsonProperty("@SequenceNmbr")]
                public string SequenceNmbr { get; set; }

                [JsonProperty("@Target")]
                public string Target { get; set; }

                [JsonProperty("@Version")]
                public string Version { get; set; }

                [JsonProperty("@xmlns")]
                public string xmlns { get; set; }

                [JsonProperty("Success")]
                public object Success { get; set; }

                [JsonProperty("VehMatchedLocs")]
                public VehMatchedLocs VehMatchedLocs { get; set; }
            }

            public class RefPoint
            {
                [JsonProperty("@CountryCode")]
                public string CountryCode { get; set; }

                [JsonProperty("#text")]
                public string text { get; set; }
            }

            public class Root
            {
                [JsonProperty("env:Envelope")]
                public EnvEnvelope envEnvelope { get; set; }
            }

            public class StateProv
            {
                [JsonProperty("@StateCode")]
                public string StateCode { get; set; }
            }

            public class Telephone
            {
                [JsonProperty("@PhoneTechType")]
                public string PhoneTechType { get; set; }

                [JsonProperty("@AreaCityCode")]
                public string AreaCityCode { get; set; }

                [JsonProperty("@PhoneNumber")]
                public string PhoneNumber { get; set; }
            }

            public class VehLocSearchCriterion
            {
                [JsonProperty("RefPoint")]
                public RefPoint RefPoint { get; set; }
            }

            public class VehMatchedLoc
            {
                [JsonProperty("LocationDetail")]
                public LocationDetail LocationDetail { get; set; }

                [JsonProperty("VehLocSearchCriterion")]
                public VehLocSearchCriterion VehLocSearchCriterion { get; set; }
            }

            public class VehMatchedLocs
            {
                [JsonProperty("VehMatchedLoc")]
                public List<VehMatchedLoc> VehMatchedLoc { get; set; }
            }


        }
        public class LocationsFromJson
        {
            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
            public class Body
            {
                [JsonProperty("proc")]
                public Proc proc { get; set; }
            }

            public class Border
            {
                [JsonProperty("@spacing")]
                public string spacing { get; set; }

                [JsonProperty("@padding")]
                public string padding { get; set; }

                [JsonProperty("@rules")]
                public string rules { get; set; }

                [JsonProperty("@frame")]
                public string frame { get; set; }
            }

            public class Branch
            {
                [JsonProperty("@name")]
                public string name { get; set; }

                [JsonProperty("@label")]
                public string label { get; set; }

                [JsonProperty("@class")]
                public string @class { get; set; }

                [JsonProperty("@toc-level")]
                public string toclevel { get; set; }

                [JsonProperty("leaf")]
                public Leaf leaf { get; set; }
            }

            public class Colgroup
            {
                [JsonProperty("colspec")]
                public List<Colspec> colspec { get; set; }
            }

            public class Colspec
            {
                [JsonProperty("@column")]
                public string column { get; set; }

                [JsonProperty("@width")]
                public string width { get; set; }

                [JsonProperty("@name")]
                public string name { get; set; }

                [JsonProperty("@type")]
                public string type { get; set; }

                [JsonProperty("@align")]
                public string align { get; set; }
            }

            public class Colspecs
            {
                [JsonProperty("@columns")]
                public string columns { get; set; }

                [JsonProperty("colgroup")]
                public Colgroup colgroup { get; set; }
            }

            public class Datum
            {
                [JsonProperty("@name")]
                public string name { get; set; }

                [JsonProperty("@label")]
                public string label { get; set; }

                [JsonProperty("@type")]
                public string type { get; set; }

                [JsonProperty("@class")]
                public string @class { get; set; }

                [JsonProperty("@sasformat")]
                public string sasformat { get; set; }

                [JsonProperty("@precision")]
                public string precision { get; set; }

                [JsonProperty("@scale")]
                public string scale { get; set; }

                [JsonProperty("@unformatted_type")]
                public string unformatted_type { get; set; }

                [JsonProperty("@unformatted_width")]
                public string unformatted_width { get; set; }

                [JsonProperty("@row")]
                public string row { get; set; }

                [JsonProperty("@column")]
                public string column { get; set; }

                [JsonProperty("label")]
                public string label2 { get; set; }

                [JsonProperty("value")]
                public object value { get; set; }

                [JsonProperty("@raw-value")]
                public string rawvalue { get; set; }

                [JsonProperty("@unformatted_value")]
                public string unformatted_value { get; set; }
            }

            public class Head
            {
                [JsonProperty("meta")]
                public Meta meta { get; set; }
            }

            public class Header
            {
                [JsonProperty("@name")]
                public string name { get; set; }

                [JsonProperty("@label")]
                public string label { get; set; }

                [JsonProperty("@type")]
                public string type { get; set; }

                [JsonProperty("@class")]
                public string @class { get; set; }

                [JsonProperty("@unformatted_type")]
                public string unformatted_type { get; set; }

                [JsonProperty("@unformatted_width")]
                public string unformatted_width { get; set; }

                [JsonProperty("@row")]
                public string row { get; set; }

                [JsonProperty("@column")]
                public string column { get; set; }

                [JsonProperty("label")]
                public string label2 { get; set; }

                [JsonProperty("value")]
                public string value { get; set; }
            }

            public class Label
            {
                [JsonProperty("@name")]
                public string name { get; set; }
            }

            public class Leaf
            {
                [JsonProperty("@name")]
                public string name { get; set; }

                [JsonProperty("@label")]
                public string label { get; set; }

                [JsonProperty("@class")]
                public string @class { get; set; }

                [JsonProperty("@toc-level")]
                public string toclevel { get; set; }

                [JsonProperty("output")]
                public Output output { get; set; }
            }

            public class Meta
            {
                [JsonProperty("@operator")]
                public string @operator { get; set; }
            }

            public class Odsxml
            {
                [JsonProperty("head")]
                public Head head { get; set; }

                [JsonProperty("body")]
                public Body body { get; set; }
            }

            public class Output
            {
                [JsonProperty("@name")]
                public string name { get; set; }

                [JsonProperty("@label")]
                public string label { get; set; }

                [JsonProperty("@clabel")]
                public string clabel { get; set; }

                [JsonProperty("output-object")]
                public OutputObject outputobject { get; set; }
            }

            public class OutputBody
            {
                [JsonProperty("row")]
                public List<Row> row { get; set; }
            }

            public class OutputHead
            {
                [JsonProperty("row")]
                public Row row { get; set; }
            }

            public class OutputObject
            {
                [JsonProperty("@type")]
                public string type { get; set; }

                [JsonProperty("@class")]
                public string @class { get; set; }

                [JsonProperty("style")]
                public Style style { get; set; }

                [JsonProperty("colspecs")]
                public Colspecs colspecs { get; set; }

                [JsonProperty("output-head")]
                public OutputHead outputhead { get; set; }

                [JsonProperty("output-body")]
                public OutputBody outputbody { get; set; }
            }

            public class Proc
            {
                [JsonProperty("@name")]
                public string name { get; set; }

                [JsonProperty("label")]
                public Label label { get; set; }

                [JsonProperty("title")]
                public Title title { get; set; }

                [JsonProperty("branch")]
                public Branch branch { get; set; }
            }

            public class Root
            {
                [JsonProperty("odsxml")]
                public Odsxml odsxml { get; set; }
            }

            public class Row
            {
                [JsonProperty("header")]
                public List<Header> header { get; set; }

                [JsonProperty("data")]
                public List<Datum> data { get; set; }
            }

            public class Style
            {
                [JsonProperty("border")]
                public Border border { get; set; }
            }

            public class Title
            {
                [JsonProperty("@type")]
                public string type { get; set; }

                [JsonProperty("@class")]
                public string @class { get; set; }

                [JsonProperty("@unformatted_type")]
                public string unformatted_type { get; set; }

                [JsonProperty("@unformatted_width")]
                public string unformatted_width { get; set; }

                [JsonProperty("@toc-level")]
                public string toclevel { get; set; }

                [JsonProperty("$")]
                public string test { get; set; }
            }



        }

        public class VehiclesFromJson
        {
            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
            public class Root
            {
                [JsonProperty("vehicles")]
                public List<Vehicle> vehicles { get; set; }
            }

            public class Vehicle
            {
                [JsonProperty("Brand")]
                public string Brand { get; set; }

                [JsonProperty("Country")]
                public string Country { get; set; }

                [JsonProperty("SIPPCode")]
                public string SIPPCode { get; set; }

                [JsonProperty("OTAVehClassSize")]
                public string OTAVehClassSize { get; set; }

                [JsonProperty("OTAVehCategory")]
                public string OTAVehCategory { get; set; }

                [JsonProperty("ExampleModel")]
                public string ExampleModel { get; set; }

                [JsonProperty("EHIWebName")]
                public string EHIWebName { get; set; }

                [JsonProperty("EHIWebVehicleDescription")]
                public string EHIWebVehicleDescription { get; set; }

                [JsonProperty("OTADoorCount")]
                public string OTADoorCount { get; set; }

                [JsonProperty("Transmission")]
                public string Transmission { get; set; }

                [JsonProperty("OTADriveType")]
                public string OTADriveType { get; set; }

                [JsonProperty("AirConditioning")]
                public string AirConditioning { get; set; }

                [JsonProperty("OTAFuelType")]
                public string OTAFuelType { get; set; }

                [JsonProperty("PassengerCapacity")]
                public string PassengerCapacity { get; set; }

                [JsonProperty("LuggageCapacity")]
                public string LuggageCapacity { get; set; }

                [JsonProperty("SmallCase")]
                public string SmallCase { get; set; }

                [JsonProperty("LargeCase")]
                public string LargeCase { get; set; }

                [JsonProperty("LargeImage")]
                public string LargeImage { get; set; }

                [JsonProperty("SmallImage")]
                public string SmallImage { get; set; }

                [JsonProperty("VehicleFeatures")]
                public string VehicleFeatures { get; set; }

                [JsonProperty("")]
                public string data { get; set; }
            }


        }
    }
}
