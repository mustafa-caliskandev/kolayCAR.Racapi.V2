using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.WheelsysResponseBase;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Wheelsys
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this Station location) =>
       location != null ? new CommonModels.Location
       {
           LocationId = 0,
           LocationCode = location.code,
           LocationName = location.name,
           IsPickup = true,
           Address = location.StationInformation.Address,
           IsAirport = location.StationInformation.StationType == "Airport" ? true : false,
           Coordinate = new CommonModels.Coordinate { Latitude = location.lat, Longitude = location.@long }
       }
       : null;

        public static List<CommonModels.Location> Map(this List<Station> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
