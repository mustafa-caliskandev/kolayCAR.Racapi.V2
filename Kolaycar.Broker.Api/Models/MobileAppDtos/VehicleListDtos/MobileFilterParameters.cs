using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels.Enums;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.VehicleListDtos
{
    public class MobileFilterParameters
    {
        public MobileDataTypes DataType { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }
    }
}
