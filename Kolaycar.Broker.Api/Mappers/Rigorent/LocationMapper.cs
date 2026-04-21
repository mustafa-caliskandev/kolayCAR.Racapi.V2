using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Rigorent
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this RigorentLocation location) =>
            location != null ? new CommonModels.Location
            {
                LocationId = location.Kayit_No.ToIntNullSafe(),
                LocationCode = location.Kayit_No,
                LocationName = location.Bolge,
                PhoneNumber = location.Phone,
                IataCode = location.IATA
            }
            : null;

        public static List<CommonModels.Location> Map(this List<RigorentLocation> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
