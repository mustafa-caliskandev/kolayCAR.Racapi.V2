using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Sixt.Response
{
    public class LocationResponseBase
    {
        [JsonProperty("?xml")]
        public Xml Xml { get; set; }
        public SIXTTURKEYWEBSERVICES SIXTTURKEYWEBSERVICES { get; set; }
    }

    public class HOURSINFO
    {
        public WEEKDAYSOPENHOURS WEEKDAYSOPENHOURS { get; set; }
        public WEEKDAYSCLOSEHOURS WEEKDAYSCLOSEHOURS { get; set; }
        public SATURDAYOPENHOURS SATURDAYOPENHOURS { get; set; }
        public SATURDAYCLOSEHOURS SATURDAYCLOSEHOURS { get; set; }
        public SUNDAYOPENHOURS SUNDAYOPENHOURS { get; set; }
        public SUNDAYCLOSEHOURS SUNDAYCLOSEHOURS { get; set; }
    }

    public class SATURDAYCLOSEHOURS
    {
        public string INFO { get; set; }
        public string HOURS { get; set; }
    }

    public class SATURDAYOPENHOURS
    {
        public string INFO { get; set; }
        public string HOURS { get; set; }
    }

    public class STATION
    {
        public string ID { get; set; }
        public string CODE { get; set; }
        public string NAME { get; set; }
        public string ADDRESS { get; set; }
        public string MANAGER { get; set; }
        public string PHONE { get; set; }
        public string EMAIL { get; set; }
        public string TYPE { get; set; }
        public HOURSINFO HOURSINFO { get; set; }
    }

    public class STATIONS
    {
        [JsonProperty("@token")]
        public string Token { get; set; }

        [JsonProperty("@time")]
        public string Time { get; set; }
        public List<STATION> STATION { get; set; }
    }

    public class SUNDAYCLOSEHOURS
    {
        public string INFO { get; set; }
        public string HOURS { get; set; }
    }

    public class SUNDAYOPENHOURS
    {
        public string INFO { get; set; }
        public string HOURS { get; set; }
    }

    public class WEEKDAYSCLOSEHOURS
    {
        public string INFO { get; set; }
        public string HOURS { get; set; }
    }

    public class WEEKDAYSOPENHOURS
    {
        public string INFO { get; set; }
        public string HOURS { get; set; }
    }

}
