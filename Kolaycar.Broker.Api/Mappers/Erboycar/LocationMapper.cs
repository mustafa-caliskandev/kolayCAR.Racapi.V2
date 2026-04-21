using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Erboycar
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this ErboycarResponseBase.Location location) =>
            location != null ? new CommonModels.Location
            {
                LocationId = 0,
                LocationCode = location.code,
                LocationName = location.name,
                Address = location.address,
                PhoneNumber = location.phone,
                MailAddress = location.email,
                IsOffice = location.pickup_location == "ofis"
            } : null;

        public static List<CommonModels.Location> Map(this List<ErboycarResponseBase.Location> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
