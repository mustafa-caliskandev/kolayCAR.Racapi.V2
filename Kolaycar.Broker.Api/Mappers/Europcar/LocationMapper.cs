using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Europcar
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this station location) =>
            location != null ? new CommonModels.Location
            {
                LocationId = 0,
                LocationCode = location.code,
                LocationName = location.name,
                MailAddress = location.mail,
                PhoneNumber = location.phone,
                Coordinate = new CommonModels.Coordinate() { Latitude = location.lat, Longitude = location.longLatitude },
                IsPickup = true,
            }
            : null;

        public static List<CommonModels.Location> Map(this List<station> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
        public static CommonModels.Location Map(this EuropcarLocation location) =>
          location != null ? new CommonModels.Location
          {
              LocationId = 0,
              LocationCode = location.requestStationCode,
              LocationName = location.stationName,
              MailAddress = location.stationEmail,
              PhoneNumber = location.stationPhone,
              Coordinate = new CommonModels.Coordinate() { Latitude = location.latitude, Longitude = location.longitude },
              IsPickup = true,
          }
          : null;

        public static List<CommonModels.Location> Map(this List<EuropcarLocation> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
