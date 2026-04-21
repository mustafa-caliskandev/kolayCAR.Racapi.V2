using KolayCAR.Broker.Domain.Models.Sixt.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Sixt
{
    public static class LocationMapper
    {
        public static List<CommonModels.Location> Map(this List<STATION> locations)
        {
            var _locations = new List<CommonModels.Location>();

            foreach (var location in locations)
            {
                _locations.Add(location.Map());
            }

            return _locations;
        }

        private static CommonModels.Location Map(this STATION apiLocation)
        {
            return new CommonModels.Location
            {
                LocationId = apiLocation.ID.ToIntNullSafe(),
                LocationCode = apiLocation.CODE + "-" + apiLocation.ID.ToStringNullSafe(),
                LocationName = apiLocation.NAME,
                Address = apiLocation.ADDRESS,
                PhoneNumber = apiLocation.PHONE,
                MailAddress = apiLocation.EMAIL
            };
        }
    }

}
