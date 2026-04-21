using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Sixt.Response
{
    public class VehicleQueryResponseBase
    {
        [JsonProperty("?xml")]
        public Xml Xml { get; set; }
        public SIXTTURKEYWEBSERVICES SIXTTURKEYWEBSERVICES { get; set; }
    }
    public class AVAILABILITY
    {
        public string STATUS { get; set; }
        public string CODE { get; set; }
        public string TITLE { get; set; }
    }

    public class CALCULATEINFO
    {
        public string STATUS { get; set; }
        public string INFO { get; set; }
    }

    public class EXTRA
    {
        public string NAME { get; set; }
        public string CODE { get; set; }
        public CALCULATEINFO CALCULATEINFO { get; set; }
        public string PRICE { get; set; }
        public string MAXDAY { get; set; }
        public string CURRENCY { get; set; }
    }

    public class EXTRAS
    {
        public List<EXTRA> EXTRA { get; set; }
    }

    public class INCLUDE
    {
        public string NAME { get; set; }
        public string CODE { get; set; }
    }

    public class INCLUDED
    {
        public List<INCLUDE> INCLUDE { get; set; }
    }

    public class INFORMATIONS
    {
        public string AVAILABILITY { get; set; }
        public string DAILYPRICE { get; set; }
        public string CURRENCY { get; set; }
        public string GEAR { get; set; }
        public string FUELTYPE { get; set; }
        public string AIRCONDITION { get; set; }
        public string INCLUDED { get; set; }
        public string INSURANCES { get; set; }
        public string EXTRAS { get; set; }
        public string DAILYKMLIMIT { get; set; }
        public string MAXKMLIMIT { get; set; }
        public string CALCULATEINFO { get; set; }
        public string ONEWAY { get; set; }
        public string PAYMENTOPTIONS { get; set; }
    }

    public class INSURANCE
    {
        public string NAME { get; set; }
        public string CODE { get; set; }
        public CALCULATEINFO CALCULATEINFO { get; set; }
        public string PRICE { get; set; }
        public string MAXDAY { get; set; }
        public string CURRENCY { get; set; }
    }

    public class INSURANCES
    {
        public List<INSURANCE> INSURANCE { get; set; }
        //public INSURANCE INSURANCE { get; set; }
    }

    public class OPTION
    {
        public string CODE { get; set; }
        public string TITLE { get; set; }
    }

    public class PAYMENTOPTIONS
    {
        public OPTION OPTION { get; set; }
    }

    public class PICKUP
    {
        public string ID { get; set; }
        public string CODE { get; set; }
        public string DATE { get; set; }
        public string TIME { get; set; }
        public string HOUR { get; set; }
        public string MINUTE { get; set; }
    }

    public class RETURN
    {
        public string ID { get; set; }
        public string CODE { get; set; }
        public string NAME { get; set; }
        public string DATE { get; set; }
        public string HOUR { get; set; }
        public string MINUTE { get; set; }
        public string TIME { get; set; }
    }
}
