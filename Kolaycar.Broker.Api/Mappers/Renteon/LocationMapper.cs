using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using System.Linq;
using Location = KolayCAR.Broker.Domain.Models.Location;

namespace KolayCAR.Broker.API.Mappers.Renteon
{
    public static class LocationMapper
    {
        public static Location Map(RenteonResponseBase.Office office, RenteonResponseBase.Location location) =>
            office != null ? new Location
            {
                LocationId = office.OfficeId.ToIntNullSafe(),
                LocationCode = office.LocationCode,
                LocationName = location.Name,
                IsPickup = office.IsPickupOffice,
                IsAirport = location.Type == "Airport",
                Address = location.Path,
                CountryCode = location.CountryCode
            }
            : null;

        public static List<Location> Map(this RenteonResponseBase.RenteonLocationResponseBase response)
        {
            var _locations = new List<Location>();

            if (response != null && response.Offices.Count != 0)
                foreach (var office in response.Offices)
                    _locations.Add(Map(office, response.Locations.Where(x => x.Code == office.LocationCode).FirstOrDefault()));

            return _locations;
        }
    }
}
