using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Responses.Eren;
using System.Collections.Generic;
using System.Linq;

namespace Kolaycar.Broker.Api.Mappers.Eren
{
    public static class LocationMapper
    {
        public static List<Location> Map(this List<ErenLocationResponse> response)
        {
            if (response == null) return new List<Location>();

            return response.Select(x => new Location
            {
                LocationId = x.LocationId,
                LocationCode = x.LocationId.ToString(),
                LocationName = x.LocationName,
                Coordinate = new Coordinate
                {
                    Latitude = x.Latitude,
                    Longitude = x.Longitude
                },
                IataCode = x.IataCode,
                IsAirport = !string.IsNullOrEmpty(x.IataCode),
                IsPickup = true,
                CountryCode = x.Country?.CountryId.ToString(),
                CountryName = x.Country?.CountryName,
                CityCode = x.City?.CityId.ToString(),
                CityName = x.City?.CityName,
                Address = x.Office?.OfficeAddress,
                PhoneNumber = x.Office?.OfficePhone,
                MailAddress = x.Office?.OfficeEmail
            }).ToList();
        }
    }
}
