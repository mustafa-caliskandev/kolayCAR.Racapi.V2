using System.Runtime.Serialization;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.VehicleListDtos
{
    public class VehiclePromotion
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public int? DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
    }
}
