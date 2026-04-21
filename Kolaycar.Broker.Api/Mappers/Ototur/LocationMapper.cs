using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.OtoturResponseBase;
using CommonModels = KolayCAR.Broker.Domain.Models;
namespace KolayCAR.Broker.API.Mappers.Ototur
{
    public static class LocationMapper
    {

        public static CommonModels.Location Map(this Content location) =>
        location != null ? new CommonModels.Location
        {
            LocationId = location.id.ToIntNullSafe(),
            LocationCode = location.id,
            LocationName = location.name,
            IsPickup = true,
            Address = location.adress,
            IsAirport = location.isAirport.ToBoolNullSafe()
        }
        : null;

        public static List<CommonModels.Location> Map(this List<Content> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
