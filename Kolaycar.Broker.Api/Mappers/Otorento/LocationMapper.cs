using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.OtorentoResponseBase;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Otorento
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this SearchLocations location) =>
               location != null ? new CommonModels.Location
               {
                   LocationCode = location.LocationId.ToString(),
                   LocationName = location.LocationTextTr,
               }
               : null;
        //public static Location Map(this OtorentoResponseBase.SearchLocations location) =>
        //       location != null ? new Location
        //       {
        //           Location_ID = location.LocationId,
        //           Location_Name = location.LocationTextTr,
        //           City = location.City,
        //           Country = location.Country

        //       }
        //: null;

        public static List<CommonModels.Location> Map(this List<SearchLocations> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
