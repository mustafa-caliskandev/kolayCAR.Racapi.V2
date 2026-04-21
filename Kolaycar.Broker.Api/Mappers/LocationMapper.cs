using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this Location location) =>
              location != null ? new CommonModels.Location
              {
                  LocationId = location.Id,
                  LocationCode = location.Id.TrimNullSafe(),
                  CountryId = location.Countryid,
                  CityId = location.Cityid,
                  LocationName = location.Locationname.TrimNullSafe(),
                  IataCode = location.Iata.TrimNullSafe(),
                  IsAirport = location.Airport ?? false,
                  IsPickup = location.Ispickup ?? false,
                  MailAddress = location.Mailaddress.TrimNullSafe(),
                  Address = location.Address.TrimNullSafe(),
                  PhoneNumber = location.Phonenumber.TrimNullSafe(),
                  Coordinate = new CommonModels.Coordinate
                  {
                      Latitude = location.Coordinatelatitude,
                      Longitude = location.Coordinatelongitude
                  }
              }
              : null;

        public static List<CommonModels.Location> Map(this List<Location> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
