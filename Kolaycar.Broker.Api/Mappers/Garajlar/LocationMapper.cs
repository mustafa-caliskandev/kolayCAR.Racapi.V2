using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.GarajlarResponseBase;

namespace KolayCAR.Broker.API.Mappers.Garajlar
{
    public static class LocationMapper
    {
        public static List<Domain.Models.Location> Map(this List<LocationListResponse> locations)
        {
            var _locations = new List<Domain.Models.Location>();

            foreach (var location in locations)
                _locations.Add(location.Map());

            return _locations;
        }

        private static Domain.Models.Location Map(this LocationListResponse location)
        {
            return new Domain.Models.Location
            {
                LocationId = 0,
                LocationCode = $"{location.code}",
                LocationName = location.web_name,
                Address = location.address,
                PhoneNumber = location.telephone,
                MailAddress = location.email,
                IsAirport = (bool)location.airport_location,
                Coordinate = new Domain.Models.Coordinate { Latitude = location.latitude, Longitude = location.longitude }
            };
        }
    }
}
