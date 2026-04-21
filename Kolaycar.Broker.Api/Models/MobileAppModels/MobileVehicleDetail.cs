using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels.Enums;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels
{
    public class MobileVehicleDetail
    {
        public int Id { get; set; }
        public MobileVehicleDetailTypes? Type { get; set; }
        public int? Order { get; set; }
        public string IconPath { get; set; }
        public string Value { get; set; }
        public string Text { get; set; }
        public bool? Active { get; set; }
    }
}
