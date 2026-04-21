using KolayCAR.Broker.API.Models.MobileAppModels.Enums;
using KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Models.MobileAppModels
{
    public class MobileVehicleSortingOption
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public int LanguageId { get; set; }
        public MobileSortingTypes SortType { get; set; }
        public string IconPath { get; set; }
        public string ValueName { get; set; }
        public string Name { get; set; }
        public bool? Default { get; set; }
        public bool? Active { get; set; }

        public virtual Language Language { get; set; }
    }
}
