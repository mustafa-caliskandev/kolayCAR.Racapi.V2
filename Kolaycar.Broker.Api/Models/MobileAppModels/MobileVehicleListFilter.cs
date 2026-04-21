using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels.Enums;
using KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels
{
    public class MobileVehicleListFilter
    {
        public int Id { get; set; }
        public MobileVehicleFutureTypes Type { get; set; }
        public int LanguageId { get; set; }
        public int Order { get; set; }
        public string IconPath { get; set; }
        public string Header { get; set; }
        public bool? Active { get; set; }

        public virtual Language Language { get; set; }
    }
}
