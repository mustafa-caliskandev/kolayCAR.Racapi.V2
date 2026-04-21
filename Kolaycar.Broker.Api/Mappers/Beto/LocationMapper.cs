using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.BetoResponseBase;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Beto
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this LocationResponse location, int languageId = 1) =>
       location != null ? new CommonModels.Location
       {
           LocationId = 0,
           LocationCode = location.KODU,
           LocationName = location.YER_TR,
           IsPickup = true,
           Address = languageId == 1 ? location.ADRES_TR : location.ADRES_EN,
           PhoneNumber = location.TEL,
           MailAddress = location.EMAIL

       }
         : null;

        public static List<CommonModels.Location> Map(this List<LocationResponse> locations, int languageId = 1)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map(languageId));

            return _locations;
        }
    }
}
