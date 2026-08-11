using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response.Vonarent;

public class VonarentVehicleResponse : VonarentResponseBase
{
    public List<VonarentVehicleItem> items { get; set; }
}

public class VonarentVehicleItem
{
    public string id { get; set; }
    public string name { get; set; }
    public string image { get; set; }
    public float price { get; set; }
    public float priceDaily { get; set; }
    public float dropPrice { get; set; }
    public string currency { get; set; }
    public int capacity { get; set; }
    public int luggageVolume { get; set; }
    public List<string> properties { get; set; }
    public string vehicleType { get; set; }
    public string fuel { get; set; }
    public string driverAge { get; set; }
    public string drivingLicenceAge { get; set; }
}
