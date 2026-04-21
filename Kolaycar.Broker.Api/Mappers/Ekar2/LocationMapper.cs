using KolayCAR.Broker.Domain.Models;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Ekar2
{
    public static class LocationMapper
    {
        public static Location Map(this Domain.Models.Response.Ekar2ResponseBase.Location location) => location != null ? new Location
        {
            LocationId = location.id,
            LocationName = location.name,
            LocationCode = location.id.ToString(),
            IsAirport = location.isAirport,
            Address = location.address,
            MailAddress = location.email,
            PhoneNumber = location.phone,
            IsOffice = false,
            Coordinate = new Coordinate { Latitude = location.latitude.ToString(), Longitude = location.longitude.ToString() }

        } : null;

        public static List<Location> Map(this List<Domain.Models.Response.Ekar2ResponseBase.Location> locations)
        {

            if (locations != null && locations.Count > 0)
            {
                var list = new List<Location>();
                foreach (var location in locations)
                    list.Add(location.Map());
                return list;
            }
            return null;
        }

    }
}
