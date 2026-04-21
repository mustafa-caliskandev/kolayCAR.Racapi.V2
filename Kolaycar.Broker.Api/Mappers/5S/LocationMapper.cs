using KolayCAR.Broker.Domain.Models.Response;
using System;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers._5S
{
    public static class LocationMapper
    {
        public static List<CommonModels.Location> Map(this BesSResponseBase.LocationResponse locationResponse)
        {
            var _locations = new List<CommonModels.Location>();
            var locations = locationResponse.data;

            if (locations != null && locations.Count != 0)
            {
                foreach (var location in locations)
                {
                    _locations.Add(Map(location));
                }
            }

            return _locations;
        }

        public static CommonModels.Location Map(BesSResponseBase.Data location)
        {
            return new CommonModels.Location
            {
                LocationId = Convert.ToInt32(location.id),
                LocationCode = location.id.ToString(),
                LocationName = location.name.tr.ToString(),
                IsPickup = true,
                Address = location.address,
                PhoneNumber = location.phone,
                MailAddress = location.email,
                Coordinate = new CommonModels.Coordinate
                {
                    Latitude = location.latitude,
                    Longitude = location.longitude
                }
            };
        }
    }
}
