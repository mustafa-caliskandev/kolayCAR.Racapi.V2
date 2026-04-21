using KolayCAR.Broker.API.Models.MobileAppDtos.ResponseDtos;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.VehicleListDtos
{
    public class LocationInfo
    {
        public MobileLocation PickupLocation { get; set; }
        public MobileLocation ReturnLocation { get; set; }
    }
}
