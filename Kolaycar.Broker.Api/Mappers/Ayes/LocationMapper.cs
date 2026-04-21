using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Ayes
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this AyesLocation location) =>
            location != null ? new CommonModels.Location
            {
                LocationId = location.id.ToIntNullSafe(),
                LocationCode = location.id,
                LocationName = location.baslik
            }
            : null;

        public static List<CommonModels.Location> Map(this List<AyesLocation> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
