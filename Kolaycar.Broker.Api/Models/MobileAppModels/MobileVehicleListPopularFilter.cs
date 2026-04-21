using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels.Enums;
using KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Models.MobileAppModels
{
    public class MobileVehicleListPopularFilter
    {
        public int Id { get; set; }
        public MobileVehicleFutureTypes Type { get; set; }
        public int LanguageId { get; set; }
        public int Order { get; set; }
        public string Value { get; set; }
        public bool? Active { get; set; }

        public virtual Language Language { get; set; }
    }
}
