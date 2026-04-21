using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.PandoraResponseBase;

namespace KolayCAR.Broker.API.Mappers.Pandora
{
    public static class LocationMapper
    {
        public static Location Map(this Office location) =>
            location != null ? new Location
            {
                LocationId = location.Id,
                LocationCode = location.Id.ToString(),
                LocationName = location.Name,
                CountryName = location.Town,
                IataCode = location.Code,
                IsPickup = true,
                Address = location.Address,
                PhoneNumber = location.Tel,
                Coordinate = new Coordinate
                {
                    Latitude = location.Latitude.ToStringNullSafe(),
                    Longitude = location.Longitude.ToStringNullSafe()
                }
            }
            : null;

        public static List<Location> Map(this List<Office> locations)
        {
            var _locations = new List<Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
