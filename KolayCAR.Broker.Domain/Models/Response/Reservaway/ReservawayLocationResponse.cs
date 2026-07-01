using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response.Reservaway;

public class ReservawayLocationResponse : ReservawayResponseBase
{
    public List<ReservawayLocationItem> locations { get; set; }
}

public class ReservawayLocationItem
{
    public int id { get; set; }
    public string iata_code { get; set; }
    public string google_place_id { get; set; }
    public string display_name { get; set; }
    public string country_code { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
}
