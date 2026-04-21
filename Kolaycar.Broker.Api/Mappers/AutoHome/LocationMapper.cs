using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.AutoHome
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this AutoHomeResponseBase.Location location) =>
            location != null ? new CommonModels.Location
            {
                //lokasyon id si string geldiği için 0 tanımlandı
                LocationId = 0,
                LocationCode = location.code,
                LocationName = location.name,
                Address = location.Address1,
                PhoneNumber = location.PhoneNo,
                Coordinate = new Coordinate { Latitude = location.latitude, Longitude = location.longitude }
            }
            : null;

        public static List<CommonModels.Location> Map(this List<AutoHomeResponseBase.Location> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }

    }
}
