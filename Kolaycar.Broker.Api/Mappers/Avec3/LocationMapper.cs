using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;
namespace KolayCAR.Broker.API.Mappers.Avec3
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this Avec3ResponseBase.LocationBase location) =>
           location != null ? new CommonModels.Location
           {
               LocationId = location.id,
               LocationCode = location.id.ToString(),
               //CountryId = 
               //CityId = 
               LocationName = location.name,
               //CountryName = location.Country,
               IataCode = location.iata_code.ToStringNullSafe(),
               IsPickup = true,
               //IsAirport = 
               Address = location.address.address_line_2.ToStringNullSafe() + " " + location.address.address_line_1.ToStringNullSafe(),
               PhoneNumber = location.phone.ToStringNullSafe(),
               Coordinate = new CommonModels.Coordinate { Latitude = location.address.latitude, Longitude = location.address.longitude }
           }
        : null;

        public static List<CommonModels.Location> Map(this List<Avec3ResponseBase.LocationBase> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
