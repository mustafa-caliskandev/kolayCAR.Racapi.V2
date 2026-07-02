using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.VehicleListDtos
{
    public class VehicleListDto
    {
        public int PickupLocationId { get; set; }
        public string SearchId { get; set; }
        public List<VehicleListFilter> Filters { get; set; }
        public List<VehicleListFastFilter> FastFilters { get; set; }
        public List<VehicleListPopularFilter> PopularFilters { get; set; }
        public List<VehicleListOrderOption> OrderOptions { get; set; }
        public List<VehicleListVendorLocation> VendorLocations { get; set; }
        //public LocationInfo LocationInfo { get; set; }
        public List<VehicleDto> Vehicles { get; set; }
        public VehicleListPopup Popup { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
    }
}
