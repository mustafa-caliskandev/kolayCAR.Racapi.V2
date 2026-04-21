using KolayCAR.Broker.API.Models.MobileAppModels.Enums;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.VehicleListDtos
{
    public class VehicleListOrderOption
    {
        public int Id{ get; set; }
        public int Order { get; set; }
        public MobileSortingTypes SortType { get; set; }
        public string IconPath { get; set; }
        public string ValueName { get; set; }
        public string Name { get; set; }
        public bool? Default { get; set; }

    }
}
