namespace KolayCAR.Broker.Domain.Models.Requests.Vonarent;

public class VonarentSelectStationRequest
{
    public string PickupStation { get; set; }
    public string PickupDate { get; set; }
    public string ReturnStation { get; set; }
    public string ReturnDate { get; set; }
    public string Currency { get; set; }
}
