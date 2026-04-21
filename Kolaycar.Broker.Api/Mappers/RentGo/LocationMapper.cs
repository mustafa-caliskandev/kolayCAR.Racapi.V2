using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Responses.RentGo;
using System.Collections.Generic;
using System.Linq;

namespace Kolaycar.Broker.Api.Mappers.RentGo
{
    public static class LocationMapper
    {
        public static List<Location> Map(this List<RentGoOffice> response)
        {
            if (response == null) return new List<Location>();

            return response.Select(x => new Location
            {
                LocationId = 0,
                LocationCode = x.BranchId,
                LocationName = x.BranchName,
                CityName = x.CityName,
                IsPickup = true,
                Address = x.Address,
                MailAddress = x.Email,
                PhoneNumber = x.Phone,
                Coordinate = x.Location?.Coordinates != null && x.Location.Coordinates.Count >= 2 ? new Coordinate
                {
                    Longitude = x.Location.Coordinates[0].ToString(System.Globalization.CultureInfo.InvariantCulture),
                    Latitude = x.Location.Coordinates[1].ToString(System.Globalization.CultureInfo.InvariantCulture)
                } : null
            }).ToList();
        }
    }
}
