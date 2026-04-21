using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class CentauroResponseBase
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
            }

            public class Root
            {
                [JsonProperty("extras")]
                public List<Extra> extras { get; set; }
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

                [JsonProperty("comments")]
                public string comments { get; set; }
            }


        }

        public class AvailabilityVehicles
        {
            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
            public class CentauroNet
            {
                [JsonProperty("COCHE")]
                public List<COCHE> COCHE { get; set; }
            }

            public class COCHE
            {
                [JsonProperty("CODIGO")]
                public string CODIGO { get; set; }

                [JsonProperty("SIPP")]
                public string SIPP { get; set; }

                [JsonProperty("DISPONIBLE")]
                public string DISPONIBLE { get; set; }

                [JsonProperty("PRECIO_POR_DIA")]
                public string PRECIO_POR_DIA { get; set; }

                [JsonProperty("PRECIO_POR_TOTAL")]
                public string PRECIO_POR_TOTAL { get; set; }
            }

            public class Root
            {
                [JsonProperty("centauro.net")]
                public CentauroNet centauronet { get; set; }
            }


        }

        public class AvailabilityExtras
        {

            public class CentauroNet
            {
                [JsonProperty("EXTRAS")]
                public EXTRAS EXTRAS { get; set; }
            }

            public class EXTRA
            {
                public string GroupCode { get; set; }

                [JsonProperty("CODEXTRA")]
                public string CODEXTRA { get; set; }

                [JsonProperty("MIN")]
                public string MIN { get; set; }

                [JsonProperty("MAX")]
                public string MAX { get; set; }

                [JsonProperty("PRECIO")]
                public string PRECIO { get; set; }

                [JsonProperty("PRECIODIARIO")]
                public string PRECIODIARIO { get; set; }

                [JsonProperty("DESCRIPCION")]
                public string DESCRIPCION { get; set; }

                [JsonProperty("DESCRIPCIONDETALLADA")]
                public string DESCRIPCIONDETALLADA { get; set; }
            }

            public class EXTRAS
            {
                [JsonProperty("EXTRA")]
                public List<EXTRA> EXTRA { get; set; }
            }

            public class Root
            {
                [JsonProperty("centauro.net")]
                public CentauroNet centauronet { get; set; }
            }




        }

        public class Reservation
        {

            public class CAR
            {
                [JsonProperty("PROVIDER_CATEGORY")]
                public string PROVIDER_CATEGORY { get; set; }
            }

            public class CONTACTDATA
            {
                [JsonProperty("NAME")]
                public string NAME { get; set; }

                [JsonProperty("SURNAME")]
                public string SURNAME { get; set; }
            }

            public class FLIGHT
            {
                [JsonProperty("NUMBER")]
                public string NUMBER { get; set; }
            }

            public class HEADER
            {
                [JsonProperty("AGENCY_ID")]
                public string AGENCY_ID { get; set; }

                [JsonProperty("SALESMAN_ID")]
                public string SALESMAN_ID { get; set; }

                [JsonProperty("CODE")]
                public string CODE { get; set; }

                [JsonProperty("WHO")]
                public WHO WHO { get; set; }

                [JsonProperty("WHERE")]
                public WHERE WHERE { get; set; }

                [JsonProperty("WHEN")]
                public WHEN WHEN { get; set; }

                [JsonProperty("FLIGHT")]
                public FLIGHT FLIGHT { get; set; }

                [JsonProperty("REMARKS")]
                public string REMARKS { get; set; }
            }

            public class PICKUP
            {
                [JsonProperty("SERVICE_POINT_PICKUP")]
                public SERVICEPOINTPICKUP SERVICE_POINT_PICKUP { get; set; }
            }

            public class RESERVATION
            {
                [JsonProperty("HEADER")]
                public HEADER HEADER { get; set; }

                [JsonProperty("CAR")]
                public CAR CAR { get; set; }

                [JsonProperty("TOTAL")]
                public TOTAL TOTAL { get; set; }

                [JsonProperty("RESPONSE")]
                public RESPONSE RESPONSE { get; set; }
            }

            public class RESPONSE
            {
                [JsonProperty("CODE")]
                public string CODE { get; set; }

                [JsonProperty("DESCRIPTION")]
                public string DESCRIPTION { get; set; }
            }

            public class RETURN
            {
                [JsonProperty("SERVICE_POINT_RETURN")]
                public SERVICEPOINTRETURN SERVICE_POINT_RETURN { get; set; }
            }

            public class Root
            {
                [JsonProperty("RESERVATION")]
                public RESERVATION RESERVATION { get; set; }
            }

            public class SERVICEPOINTPICKUP
            {
                [JsonProperty("CODE")]
                public string CODE { get; set; }
            }

            public class SERVICEPOINTRETURN
            {
                [JsonProperty("CODE")]
                public string CODE { get; set; }
            }

            public class TOTAL
            {
                [JsonProperty("NET")]
                public string NET { get; set; }
            }

            public class WHEN
            {
                [JsonProperty("CREATION_DATE")]
                public string CREATION_DATE { get; set; }

                [JsonProperty("START_DATE")]
                public string START_DATE { get; set; }

                [JsonProperty("END_DATE")]
                public string END_DATE { get; set; }
            }

            public class WHERE
            {
                [JsonProperty("PICKUP")]
                public PICKUP PICKUP { get; set; }

                [JsonProperty("RETURN")]
                public RETURN RETURN { get; set; }
            }

            public class WHO
            {
                [JsonProperty("CONTACT_DATA")]
                public CONTACTDATA CONTACT_DATA { get; set; }
            }


        }

        public class CancelReservation
        {
            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
            public class HEADER
            {
                [JsonProperty("CODE")]
                public string CODE { get; set; }
            }

            public class RESERVATION
            {
                [JsonProperty("HEADER")]
                public HEADER HEADER { get; set; }

                [JsonProperty("RESPONSE")]
                public RESPONSE RESPONSE { get; set; }
            }

            public class RESPONSE
            {
                [JsonProperty("CODE")]
                public string CODE { get; set; }

                [JsonProperty("DESCRIPTION")]
                public string DESCRIPTION { get; set; }
            }

            public class Root
            {
                [JsonProperty("RESERVATION")]
                public RESERVATION RESERVATION { get; set; }
            }


        }
    }
}