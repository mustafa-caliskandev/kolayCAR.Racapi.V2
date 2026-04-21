using Newtonsoft.Json;

namespace KolayCAR.Broker.Domain.Models.Sixt.Response
{
    public class ReservationCancelResponseBase
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class PROCESSSTATUS
        {
            [JsonConstructor]
            public PROCESSSTATUS(
                string cODE,
                string mESSAGE,
                string dETAILS
            )
            {
                this.CODE = cODE;
                this.MESSAGE = mESSAGE;
                this.DETAILS = dETAILS;
            }

            public string CODE { get; }
            public string MESSAGE { get; }
            public string DETAILS { get; }
        }

        public class RESERVATIONSTATUS
        {
            [JsonConstructor]
            public RESERVATIONSTATUS(
                [JsonProperty("@token")] string token,
                STATUS sTATUS,
                string tIMEINFO,
                string rESERVATIONCODE
            )
            {
                this.Token = token;
                this.STATUS = sTATUS;
                this.TIMEINFO = tIMEINFO;
                this.RESERVATIONCODE = rESERVATIONCODE;
            }

            [JsonProperty("@token")]
            public string Token { get; }
            public STATUS STATUS { get; }
            public string TIMEINFO { get; }
            public string RESERVATIONCODE { get; }
        }

        public class Root
        {
            [JsonConstructor]
            public Root(
                [JsonProperty("?xml")] Xml xml,
                SIXTTURKEYWEBSERVICES sIXTTURKEYWEBSERVICES
            )
            {
                this.Xml = xml;
                this.SIXTTURKEYWEBSERVICES = sIXTTURKEYWEBSERVICES;
            }

            [JsonProperty("?xml")]
            public Xml Xml { get; }
            public SIXTTURKEYWEBSERVICES SIXTTURKEYWEBSERVICES { get; }
        }

        public class SIXTTURKEYWEBSERVICES
        {
            [JsonConstructor]
            public SIXTTURKEYWEBSERVICES(
                [JsonProperty("@version")] string version,
                PROCESSSTATUS pROCESSSTATUS,
                RESERVATIONSTATUS rESERVATIONSTATUS
            )
            {
                this.Version = version;
                this.PROCESSSTATUS = pROCESSSTATUS;
                this.RESERVATIONSTATUS = rESERVATIONSTATUS;
            }

            [JsonProperty("@version")]
            public string Version { get; }
            public PROCESSSTATUS PROCESSSTATUS { get; }
            public RESERVATIONSTATUS RESERVATIONSTATUS { get; }
        }

        public class STATUS
        {
            [JsonConstructor]
            public STATUS(
                string iNFO
            )
            {
                this.INFO = iNFO;
            }

            public string INFO { get; }
        }

        public class Xml
        {
            [JsonConstructor]
            public Xml(
                [JsonProperty("@version")] string version,
                [JsonProperty("@encoding")] string encoding
            )
            {
                this.Version = version;
                this.Encoding = encoding;
            }

            [JsonProperty("@version")]
            public string Version { get; }

            [JsonProperty("@encoding")]
            public string Encoding { get; }
        }


    }


}
