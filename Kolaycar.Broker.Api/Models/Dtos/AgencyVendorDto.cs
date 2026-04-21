namespace KolayCAR.Broker.API.Models.Dtos
{
    public class AgencyVendorDto
    {
        public int AgencyId { get; set; }
        public int VendorId { get; set; }
        public int VendorType { get; set; }
        public int LocationId { get; set; }
        public bool PickupLocation { get; set; }
        public string VendorName { get; set; }
        public string ApiKey { get; set; }
        public string ApiPassword { get; set; }
        public string ApiClientId { get; set; }
        public string VendorLocationCode { get; set; }
        public string SecretKey { get; set; }
    }
}
