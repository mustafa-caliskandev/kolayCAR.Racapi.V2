using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response.Reservaway;

public class ReservawayResponseBase
{
    public string status { get; set; }
    public string message { get; set; }
    public Dictionary<string, List<string>> errors { get; set; }
}

public class ReservawayDeeplinkResponse : ReservawayResponseBase
{
    public string deeplink { get; set; }
}
