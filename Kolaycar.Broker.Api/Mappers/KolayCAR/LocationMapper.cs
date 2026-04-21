using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.KolayCAR
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this LOCATION location) =>
            location != null ? new CommonModels.Location
            {
                LocationId = location.LOCATIONID,
                LocationCode = location.LOCATIONID.ToString(),
                CountryId = location.COUNTRYID,
                CityId = location.CITYID,
                LocationName = location.LOCATIONNAME + (location.ISPICKUP ? " - (Alış L.)" : string.Empty),
                CountryName = location.COUNTRYNAME,
                IataCode = location.IATACODE,
                IsPickup = location.ISPICKUP,
                IsAirport = location.ISAIRPORT,
                IsOffice = location.LOCATIONTYPES[0].LOCATIONTYPEID == 1
            }
            : null;

        public static List<CommonModels.Location> Map(this List<LOCATION> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
