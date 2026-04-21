using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;
namespace KolayCAR.Broker.API.Mappers.Avis
{
    public static class LocationMaper
    {
        public static CommonModels.Location Map(this AvisResponseBase.Location location) =>
          location != null ? new CommonModels.Location
          {
              LocationId = location.OfficeNo,
              LocationCode = location.OfficeCode,
              LocationName = location.WebOfficeTr ?? location.OfficeCode + ' ' + location.OfficeName,
              CountryName = location.OfficeName,
              IsPickup = true,
              IsAirport = location.IsAirport,
              Address = location.PostAdress1,
              PhoneNumber = location.PhoneNumber,
              MailAddress = location.OfisMail,
              DistrictCode = location.DistrictId.ToString(),
              CityCode = location.CityId.ToString()
          }
        : null;

        public static List<CommonModels.Location> Map(this List<AvisResponseBase.Location> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
