using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels.Enums;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.VehicleListDtos
{
    public class VehicleDetail
    {
        public int? Order { get; set; }
        public MobileVehicleDetailTypes? Type { get; set; }
        public string Icon { get; set; }
        public string Value { get; set; }
        public string Text { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Info Info { get; set; }
    }
}
