using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Responses.YesOto
{
    public class YesOtoBaseResponse<T>
    {
        public T data { get; set; }
        public string message { get; set; }
        public bool success { get; set; }
        public string redirectUrl { get; set; }
    }

    public class YesOtoLocationData
    {
        public string value { get; set; }
        public string text { get; set; }
        public string slug { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public bool isAirportLocation { get; set; }
    }

    public class YesOtoLocationResponse : YesOtoBaseResponse<List<YesOtoLocationData>>
    {
    }
}
