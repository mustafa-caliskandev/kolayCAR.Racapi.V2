namespace KolayCAR.Broker.Domain.Models
{
    public class PostVendorVehicleDto
    {
        public int VendorType { get; set; }
        public string ApiKey { get; set; }
        public string ApiPassword { get; set; }
        public string ApiClientId { get; set; }
        public string LanguageCode { get; set; }
        public string CurrencyCode { get; set; }
        public int PickupLocationId { get; set; }
        public int ReturnLocationId { get; set; }
        public string PickupDate { get; set; }
        public string ReturnDate { get; set; }
        public string PickupTime { get; set; }
        public string ReturnTime { get; set; }
        public string ApiLocationCode { get; set; }
        public bool EncryptedKeys { get; set; } = false;
        public string UserToken { get; set; } = null;
        public string CouponCode { get; set; } = null;
    }
}
