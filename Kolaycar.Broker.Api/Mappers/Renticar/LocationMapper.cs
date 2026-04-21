using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Renticar.Response;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Renticar
{
    public static class LocationMapper
    {
        public static List<Location> Map(this List<LocationsResponseBase> apiLocations)
        {
            var _locations = new List<Location>();

            foreach (var item in apiLocations)
            {
                _locations.Add(item.Map());
            }

            return _locations;
        }

        private static Location Map(this LocationsResponseBase apiLocation)
        {
            return new Location
            {
                LocationId = 0,
                LocationCode = apiLocation.locationSlug,
                LocationName = apiLocation.locationName
            };
        }
    }
}
