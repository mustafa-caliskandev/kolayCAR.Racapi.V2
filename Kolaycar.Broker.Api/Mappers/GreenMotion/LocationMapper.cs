using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.GreenMotion
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this GreenMotionServicearea location) =>
               location != null ? new CommonModels.Location
               {
                   LocationCode = location.locationID,
                   LocationName = location.name
               }
               : null;

        public static CommonModels.Location Map(this GreenMotionLocationInfo location, string apiLocationCode) =>
               location != null ? new CommonModels.Location
               {
                   LocationCode = apiLocationCode,
                   LocationName = location.location_name,
                   Address = $"{location.address_1} {location.address_2} {location.address_3} {location.address_city}",
                   PhoneNumber = location.telephone,
                   MailAddress = location.email,
                   IataCode = location.iata
               }
               : null;

        public static List<CommonModels.Location> Map(this List<GreenMotionServicearea> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
