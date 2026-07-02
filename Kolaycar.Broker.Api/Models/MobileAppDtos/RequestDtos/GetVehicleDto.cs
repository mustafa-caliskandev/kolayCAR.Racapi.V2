using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.RequestDtos
{
    public class GetVehicleDto
    {
        public int VendorType { get; set; }
        public string ApiKey { get; set; }
        public string ApiPassword { get; set; }
        public string ApiClientId { get; set; }
        public string LanguageCode { get; set; }
        public string CurrencyCode { get; set; }
        public int PickupLocationId { get; set; }
        public List<int> PickupLocationIds { get; set; }
        public int ReturnLocationId { get; set; }
        public string PickupDate { get; set; }
        public string ReturnDate { get; set; }
        public string PickupTime { get; set; }
        public string ReturnTime { get; set; }
        public string UserToken { get; set; }
        public string CouponCode { get; set; }
        public string SessionCode { get; set; }
    }
}
