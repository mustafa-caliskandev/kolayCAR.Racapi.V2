using System;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;
using YesOtoModels = KolayCAR.Broker.Domain.Models.Responses.YesOto;

namespace Kolaycar.Broker.Api.Mappers.YesOto
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this YesOtoModels.YesOtoLocationData location)
        {
            if (location == null) return null;

            return new CommonModels.Location
            {
                LocationId = 0,
                LocationCode = location.value,
                LocationName = location.text,
                IsAirport = location.isAirportLocation,
                Coordinate = new CommonModels.Coordinate
                {
                    Latitude = location.latitude.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    Longitude = location.longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)
                }
            };
        }

        public static List<CommonModels.Location> Map(this List<YesOtoModels.YesOtoLocationData> locations)
        {
            var result = new List<CommonModels.Location>();

            if (locations != null && locations.Count > 0)
            {
                foreach (var location in locations)
                {
                    result.Add(location.Map());
                }
            }

            return result;
        }
    }
}
