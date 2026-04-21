using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels.Enums;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels
{
    public class MobileSortingParameter
    {
        public int Id { get; set; }
        public MobileDataTypes DataType { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public bool? Active { get; set; }
    }
}
