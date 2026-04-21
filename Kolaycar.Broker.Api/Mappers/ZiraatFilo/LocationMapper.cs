using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.ZiraatFilo
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this CommonModels.Response.ZiraatFiloResposeBase.Location location) =>
          location != null ? new CommonModels.Location
          {
              LocationId = 0,
              LocationCode = location.Location_ID,
              LocationName = location.Location_Name,
              MailAddress = location.Mail_Adress,
              PhoneNumber = location.Telephone,
              //Coordinate = new CommonModels.Coordinate() { Latitude = location.lat, Longitude = location.longLatitude },
              IsPickup = true,
          }
        : null;

        public static List<CommonModels.Location> Map(this List<CommonModels.Response.ZiraatFiloResposeBase.Location> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
