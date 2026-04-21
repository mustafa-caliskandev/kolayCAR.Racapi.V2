using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Circular
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this CircularResponseBase.Destination location) =>
        location != null ? new CommonModels.Location
        {
            LocationId = 0,
            LocationCode = location.id.ToString(),
            //CountryId = 
            //CityId = 
            //IataCode=location.a,
            LocationName = location.name,
            IsPickup = true,
            Address = location.address,
            PhoneNumber = location.phone,
            MailAddress = location.email,
            CountryName = location.city,
            Coordinate = location.coord != null & location.coord.Contains(',') ?
            new CommonModels.Coordinate
            {
                Latitude = location.coord.Split(',')[0].Trim(),
                Longitude = location.coord.Split(',')[1].Trim()
            } : null,

        }
          : null;

        public static List<CommonModels.Location> Map(this List<CircularResponseBase.Destination> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
