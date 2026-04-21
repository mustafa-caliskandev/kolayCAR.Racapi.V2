namespace KolayCAR.Broker.Domain.Models.Renticar.Response
{
    public class LocationsResponseBase
    {
        public string locationId { get; set; }
        public string locationSlug { get; set; }
        public string locationName { get; set; }
        public string city { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }
}
