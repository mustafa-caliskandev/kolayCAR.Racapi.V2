using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.VehicleListDtos
{
    public class VehicleListFilter
    {
        public int Order { get; set; }
        public string Type { get; set; }
        //public string Icon { get; set; }
        public string Header { get; set; }
        public List<VehicleListFilterDetail> Children { get; set; }
    }
}
