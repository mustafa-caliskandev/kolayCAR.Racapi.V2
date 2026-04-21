using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Sixt2
{
    public static class LocationMapper
    {
        public static List<Domain.Models.Location> Map(this List<SixtLocationListResponse> locations)
        {
            var _locations = new List<Domain.Models.Location>();
            foreach (var location in locations)
            {
                _locations.Add(location.Map());
            }

            return _locations;
        }

        private static Domain.Models.Location Map(this SixtLocationListResponse location)
        {
            return new Domain.Models.Location
            {
                LocationId = 0,
                LocationCode = $"{location.id}~{location.code}",
                LocationName = location.name,
                Address = location.address,
                PhoneNumber = location.phone,
                MailAddress = location.email
            };
        }

        public static List<Domain.Models.Location> Map(this List<SixtLocationItem> locations)
        {
            var _locations = new List<Domain.Models.Location>();
            foreach (var location in locations)
            {
                _locations.Add(location.Map());
            }

            return _locations;
        }

        private static Domain.Models.Location Map(this SixtLocationItem location)
        {
            return new Domain.Models.Location
            {
                LocationId = 0,
                LocationCode = $"{location.Id}~{location.Code}",
                LocationName = location.Name,
                Address = location.Address,
                PhoneNumber = location.Phone,
                MailAddress = location.Email
            };
        }
    }
}
