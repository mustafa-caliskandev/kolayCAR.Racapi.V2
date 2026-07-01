using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response.Reservaway;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Reservaway
{
    public static class LocationMapper
    {
        public static List<Location> Map(this List<ReservawayLocationItem> apiLocations)
        {
            var locations = new List<Location>();
            if (apiLocations == null)
                return locations;

            foreach (var apiLocation in apiLocations)
            {
                locations.Add(new Location
                {
                    LocationId = apiLocation.id,
                    LocationCode = apiLocation.id.ToString(),
                    IataCode = apiLocation.iata_code.ToStringNullSafe(),
                    LocationName = apiLocation.display_name.ToStringNullSafe(),
                    CountryCode = apiLocation.country_code.ToStringNullSafe(),
                    IsPickup = true,
                    IsAirport = !string.IsNullOrWhiteSpace(apiLocation.iata_code),
                    Coordinate = new Coordinate
                    {
                        Latitude = apiLocation.latitude.ToStringNullSafe(),
                        Longitude = apiLocation.longitude.ToStringNullSafe()
                    }
                });
            }

            return locations;
        }
    }
}
