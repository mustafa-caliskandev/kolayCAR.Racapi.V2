
using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Enuygun
{
    public static class LocationMapper
    {

        public static List<CommonModels.Location> Map(this List<EnuygunResponse.Locations.Location> apiLocations)
        {
            var _locations = new List<CommonModels.Location>();

            foreach (var location in apiLocations)
            {
                _locations.Add(location.Map());

            }

            return _locations;


        }

        public static CommonModels.Location Map(this EnuygunResponse.Locations.Location apiLocation)
        {
            return new CommonModels.Location
            {
                LocationId = 0,
                LocationName = apiLocation.Name + " / " + apiLocation.City,
                //LocationName = apiLocation.Slug,
                LocationCode = apiLocation.Slug,
                CityCode = apiLocation.City,
                CountryName = apiLocation.Country,
                Coordinate = new CommonModels.Coordinate
                {
                    Latitude = apiLocation.Latitude,
                    Longitude = apiLocation.Longitude,
                }

            };
        }
    }
}
