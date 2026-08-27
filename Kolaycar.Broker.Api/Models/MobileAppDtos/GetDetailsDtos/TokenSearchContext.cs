using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.GetDetailsDtos
{
    [DataContract(Name = "token-search-context"), Serializable]
    public class TokenSearchContext
    {
        [DataMember(Order = 1, Name = "pickupLocationId"), JsonProperty(PropertyName = "pickupLocationId")]
        public int PickupLocationId { get; set; }

        [DataMember(Order = 2, Name = "returnLocationId"), JsonProperty(PropertyName = "returnLocationId")]
        public int ReturnLocationId { get; set; }

        [DataMember(Order = 3, Name = "pickupDate"), JsonProperty(PropertyName = "pickupDate")]
        public string PickupDate { get; set; }

        [DataMember(Order = 4, Name = "pickupTime"), JsonProperty(PropertyName = "pickupTime")]
        public string PickupTime { get; set; }

        [DataMember(Order = 5, Name = "returnDate"), JsonProperty(PropertyName = "returnDate")]
        public string ReturnDate { get; set; }

        [DataMember(Order = 6, Name = "returnTime"), JsonProperty(PropertyName = "returnTime")]
        public string ReturnTime { get; set; }

        [DataMember(Order = 7, Name = "currencyCode"), JsonProperty(PropertyName = "currencyCode")]
        public string CurrencyCode { get; set; }

        [DataMember(Order = 8, Name = "vehicleId"), JsonProperty(PropertyName = "vehicleId")]
        public int VehicleId { get; set; }

        [DataMember(Order = 9, Name = "vendorId"), JsonProperty(PropertyName = "vendorId")]
        public int VendorId { get; set; }
    }
}
