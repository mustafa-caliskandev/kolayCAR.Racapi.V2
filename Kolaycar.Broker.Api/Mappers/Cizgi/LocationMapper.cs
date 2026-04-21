using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Cizgi
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this CizgiResponseBase.Destination location) =>
        location != null ? new CommonModels.Location
        {
            LocationId = 0,
            LocationCode = location.destId.ToString(),
            //CountryId = 
            //CityId = 
            IataCode = location.airportCode,
            LocationName = location.destName,
            IsPickup = true,
            Address = location.address,
            PhoneNumber = location.phone,
            MailAddress = location.email

        }
          : null;

        public static List<CommonModels.Location> Map(this List<CizgiResponseBase.Destination> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
