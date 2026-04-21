using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Eganis
{
    public static class LocationMapper
    {
        public static List<CommonModels.Location> Map(this List<EganisResponseBase.LocationResponse.Location> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations?.Count > 0)
            {
                foreach (var location in locations)
                {
                    _locations.Add(location.Map());
                }
            }

            return _locations;

        }

        public static CommonModels.Location Map(this EganisResponseBase.LocationResponse.Location location) =>
            location != null ? new CommonModels.Location
            {
                LocationId = location.locationId,
                LocationCode = location.locationId.ToString(),
                LocationName = location.locationName,
                Address = location.address,
                MailAddress = location.eMail,
                Coordinate = new CommonModels.Coordinate
                {
                    Latitude = location.latitude.ToString(),
                    Longitude = location.longitude.ToString(),
                },
                PhoneNumber = location.phoneNr
            } : null;
    }
}
