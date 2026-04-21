using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Sixt.Response
{
    public class ResponseBase
    {
    }

    public class PROCESSSTATUS
    {
        public string CODE { get; set; }
        public string MESSAGE { get; set; }
        public string DETAILS { get; set; }
    }

    public class SIXTTURKEYWEBSERVICESFORVEHICLELIST
    {
        [JsonProperty("@version")]
        public string Version { get; set; }
        public PROCESSSTATUS PROCESSSTATUS { get; set; }
        public STATIONS STATIONS { get; set; }
        public List<VEHICLES> VEHICLES { get; set; }
        public PICKUP PICKUP { get; set; }
        public RETURN RETURN { get; set; }
        public string ONEWAY { get; set; }
        public string CURRENCY { get; set; }
        public PAYMENTOPTIONS PAYMENTOPTIONS { get; set; }
        public RESERVATIONSTATUS RESERVATIONSTATUS { get; set; }
    }

    public class SIXTTURKEYWEBSERVICES
    {
        [JsonProperty("@version")]
        public string Version { get; set; }
        public PROCESSSTATUS PROCESSSTATUS { get; set; }
        public STATIONS STATIONS { get; set; }
        public VEHICLES VEHICLES { get; set; }
        public PICKUP PICKUP { get; set; }
        public RETURN RETURN { get; set; }
        public string ONEWAY { get; set; }
        public string CURRENCY { get; set; }
        public PAYMENTOPTIONS PAYMENTOPTIONS { get; set; }
        public RESERVATIONSTATUS RESERVATIONSTATUS { get; set; }
    }

    public class Xml
    {
        [JsonProperty("@version")]
        public string Version { get; set; }

        [JsonProperty("@encoding")]
        public string Encoding { get; set; }
    }
    public class VEHICLES
    {
        [JsonProperty("@token")]
        public string Token { get; set; }

        [JsonConverter(typeof(CustomArrayConverter<VEHICLE>))]
        public List<VEHICLE> VEHICLE { get; set; }
        public string RENTALINFORMATION { get; set; }
        public INFORMATIONS INFORMATIONS { get; set; }
    }
    public class VEHICLE
    {
        public string ID { get; set; }
        public string NID { get; set; }
        public AVAILABILITY AVAILABILITY { get; set; }
        public string DAILYPRICE { get; set; }
        public string AGELIMIT { get; set; }
        public string DEPOSIT { get; set; }
        public string DRIVERLICANSELIMIT { get; set; }
        public string GROUPNAME { get; set; }
        public string SAMPLECARTITLES { get; set; }
        public string SAMPLETITLES { get; set; }
        public string PICTURE { get; set; }
        public PROPERTIES PROPERTIES { get; set; }
        public INCLUDED INCLUDED { get; set; }
        public INSURANCES INSURANCES { get; set; }
        public EXTRAS EXTRAS { get; set; }
    }

    public class PROPERTIES
    {
        public string CLASS { get; set; }
        public string TYPE { get; set; }
        public string GEAR { get; set; }
        public string FUELTYPE { get; set; }
        public string AIRCONDITION { get; set; }
        public string DOORCOUNT { get; set; }
        public string PASSENGERS { get; set; }
    }
}
