using KolayCAR.Broker.API.Models;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers
{
    public static class LocationVendorMapper
    {
        public static List<CommonModels.LocationVendor> Map(this List<Locationvendor> locationvendors)
        {
            var locationVendorList = new List<CommonModels.LocationVendor>();

            if (locationvendors != null && locationvendors.Count > 0)
            {
                foreach (var item in locationvendors)
                {
                    locationVendorList.Add(item.Map());
                }
            }

            return locationVendorList;
        }
        public static CommonModels.LocationVendor Map(this Locationvendor locationvendor)
        {
            return new CommonModels.LocationVendor
            {
                Id = locationvendor.Id,
                Vendorid = locationvendor.Vendorid,
                Locallocationid = locationvendor.Locallocationid,
                Active = locationvendor.Active,
                Apilocationname = locationvendor.Apilocationname,
                Isoffice = locationvendor.Isoffice,
                Ispickup = locationvendor.Ispickup,
                Locationcode = locationvendor.Locationcode,
                Locationid = locationvendor.Locationid,
                DistrictCode = locationvendor.DistrictCode,
                CityCode = locationvendor.CityCode
            };
        }
    }
}
