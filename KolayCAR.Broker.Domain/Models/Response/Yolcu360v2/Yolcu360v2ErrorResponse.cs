using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response.Yolcu360v2;

public class Yolcu360v2ErrorResponse
{
    public int? code { get; set; }
    public string description { get; set; }
    public Dictionary<string, string> details { get; set; }
}
