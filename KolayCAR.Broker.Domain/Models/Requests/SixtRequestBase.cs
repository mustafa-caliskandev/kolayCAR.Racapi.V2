
using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class SixtRequestBase
    {
        public class SixtLoginRequest
        {
            public string email { get; set; }
            public string password { get; set; }
        }
        public class SixtPostReservationRequest
        {
            public string unid { get; set; }
            public SixtReservation reservation { get; set; }
            public string vehicle_group { get; set; }
            public List<SixtExtra> extras { get; set; }
            public SixtCustomer customer { get; set; }
            public SixtInvoice invoice { get; set; }
            public SixtLocation pickup { get; set; }
            [JsonProperty("return")]
            public SixtLocation returnLocation { get; set; }
        }
        public class SixtCancelReservationRequest
        {
            public string cancel_reason { get; set; }
        }
        public class SixtReservation
        {
            public string flight_no { get; set; }
            public string airport { get; set; }
            public string comment { get; set; }
            public string ip_address { get; set; }
            public string user_agent { get; set; }
        }
        public class SixtExtra
        {
            public string extra_code { get; set; }
            public string name { get; set; }
            public string price { get; set; }
            public int piece { get; set; }
        }
        public class SixtCustomer
        {
            public string name { get; set; }
            public string second_name { get; set; }
            public string surname { get; set; }
            //1:Erkek, 2:Kadın
            public string gender { get; set; }
            public string email { get; set; }
            public string gsm_prefix { get; set; }
            public string gsm { get; set; }
            public string id_no { get; set; }
            public string id_birth_date { get; set; }
            public string license_no { get; set; }
            public string license_date { get; set; }
            public string country { get; set; }
            public string city { get; set; }
            public string address { get; set; }
            public string nationality { get; set; }
        }
        public class SixtInvoice
        {
            public string title { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string country_name { get; set; }
            public string city_name { get; set; }
            public string address { get; set; }
            public string tax_office { get; set; }
            public string tax_no { get; set; }
        }
        public class SixtLocation
        {
            public string station_id { get; set; }
            public string station_code { get; set; }
            public string date { get; set; }
            public string time { get; set; }
        }
    }
}
