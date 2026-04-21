using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models.Response.Yolcu360v2;
using System.Collections.Generic;
using System.Linq;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Yolcu360v2
{
    public static class LocationMapper
    {
        public static List<CommonModels.Location> Map(this List<Yolcu360v2ResponseBaseDeleted.Yolcu360v2LocationResponse> locations, List<Yolcu360v2ResponseBaseDeleted.Yolcu360v2LocationDetailResponse> locationDetails)
        {
            var _locations = new List<CommonModels.Location>();
            if (locations?.Count > 0)
            {
                foreach (var location in locations)
                {
                    var locationDetail = locationDetails.FirstOrDefault(x => x.placeId == location.placeId);
                    if (locationDetail != null)
                    {
                        _locations.Add(new CommonModels.Location
                        {
                            LocationId = 0,
                            LocationCode = $"{locationDetail.point.lat.ToString()}~{locationDetail.point.lon.ToString()}~{locationDetail.placeId}",
                            LocationName = location.mainText,
                            Address = locationDetail.name
                        });
                    }
                }
            }
            return _locations;
        }
        public static List<Locationvendor> Map(this IEnumerable<Locationvendor> locations, int vendorId)
        {
            var _locations = new List<Locationvendor>();
            if (locations?.Count() > 0)
            {
                foreach (var location in locations)
                {
                    _locations.Add(new Locationvendor
                    {
                        Vendorid = vendorId,
                        Locallocationid = location.Locallocationid,
                        Locationid = location.Locationid,
                        Active = location.Active,
                        Ispickup = location.Ispickup,
                        Apilocationname = location.Apilocationname,
                        Isoffice = location.Isoffice,
                        Locationcode = location.Locationcode,
                        DistrictCode = location.DistrictCode,
                        CityCode = location.CityCode,
                        FlightCardMandatory = location.FlightCardMandatory,
                        EarliestResTime = location.EarliestResTime,
                        RateCode = location.RateCode,
                    });
                }
            }
            return _locations;
        }
    }
}
