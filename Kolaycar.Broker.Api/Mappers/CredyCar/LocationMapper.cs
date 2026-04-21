using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.CredyCar
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this CredyCarResponseBase.Location location) =>
           location != null ? new CommonModels.Location
           {
               LocationId = location.Location_ID,
               LocationCode = location.Location_ID.ToString(),
               //CountryId = 
               //CityId = 
               LocationName = location.Location_Name,
               //CountryName = location.Country,
               //IataCode = location.IATA.ToStringNullSafe(),
               IsPickup = true,
               //IsAirport = 
               Address = location.Adress//,
               //PhoneNumber = location.Telephone
           }
           : null;

        public static List<CommonModels.Location> Map(this List<CredyCarResponseBase.Location> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
