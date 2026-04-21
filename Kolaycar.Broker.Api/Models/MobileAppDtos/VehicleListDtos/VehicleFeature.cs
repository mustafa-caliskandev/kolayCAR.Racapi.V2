using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels.Enums;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.VehicleListDtos
{
    public class VehicleFeature
    {
        public int? Order { get; set; }
        public MobileVehicleFutureTypes? Type { get; set; }
        //public string Icon { get; set; }
        public string Value { get; set; }
        public string Text { get; set; }
        //public Info Info { get; set; }
    }
}
