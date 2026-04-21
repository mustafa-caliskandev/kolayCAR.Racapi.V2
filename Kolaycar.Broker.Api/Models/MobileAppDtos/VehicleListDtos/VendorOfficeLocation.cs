namespace KolayCAR.Broker.API.Models.MobileAppDtos.VehicleListDtos
{
    public class VendorOfficeLocation
    {
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string GoogleLink { get; set; }
        public string ShortAddress { get; set; }
        public string Icon { get; set; }
    }
}
