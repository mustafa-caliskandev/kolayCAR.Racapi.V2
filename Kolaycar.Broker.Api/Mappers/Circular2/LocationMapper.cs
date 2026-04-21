using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;
namespace KolayCAR.Broker.API.Mappers.Circular2
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this Circular2ResponseBase.Destination location) =>
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
            new Coordinate
            {
                Latitude = location.coord.Split(',')[0].Trim(),
                Longitude = location.coord.Split(',')[1].Trim()
            } : null,

        }
        : null;
        public static CommonModels.Location Map(this Circular2ResponseBase.LocationItem
            location) =>
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
          new Coordinate
          {
              Latitude = location.coord.Split(',')[0].Trim(),
              Longitude = location.coord.Split(',')[1].Trim()
          } : null,

      }
      : null;

        public static List<CommonModels.Location> Map(this List<Circular2ResponseBase.Destination> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
        public static List<CommonModels.Location> Map(this Dictionary<string, Circular2ResponseBase.LocationItem> data)
        {
            var _locations = new List<CommonModels.Location>();
            foreach (var item in data)
            {
                var location = item.Value;
                _locations.Add(location.Map());

            }
            return _locations;
        }
    }
}
