using KolayCAR.Broker.Domain.Models;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.CentralResponseBase;

namespace KolayCAR.Broker.API.Mappers.Central
{
    public static class LocationMapper
    {
        public static Location Map(this CentralLocation location) =>
            location != null ? new Location
            {
                LocationCode = location.code,
                LocationName = location.name,
                PhoneNumber = location.PhoneNo,
                MailAddress = location.Email,
                Address = location.Address1,
                IsAirport = location.havaalani_lokasyonu == "1"
            }
            : null;

        public static List<Location> Map(this List<CentralLocation> locations)
        {
            var _locations = new List<Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
