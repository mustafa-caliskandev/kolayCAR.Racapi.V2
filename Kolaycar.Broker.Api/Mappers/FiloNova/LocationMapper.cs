using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.FiloNova
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this FiloNovaResponseBase.Branch location) =>
          location != null ? new CommonModels.Location
          {
              LocationId = 0,
              LocationCode = location.branchId.ToString(),
              //CountryId = 
              //CityId = 
              LocationName = location.branchName,
              IsPickup = true,
              Address = location.addressDetail,
              PhoneNumber = location.telephone,
              MailAddress = location.emailaddress

          }
            : null;

        public static List<CommonModels.Location> Map(this List<FiloNovaResponseBase.Branch> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
