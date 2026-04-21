using KolayCAR.Broker.Domain.Models.Response;
using System;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Yolcu360
{
    public static class LocationMapper
    {
        public static List<CommonModels.Location> Map(this List<Yolcu360LocationResponseBase.Data> locations)
        {
            var _locations = new List<CommonModels.Location>();
            if (locations != null && locations.Count > 0)
            {
                foreach (var location in locations)
                {
                    _locations.Add(location.Map());
                }
            }
            return _locations;
        }

        public static CommonModels.Location Map(this Yolcu360LocationResponseBase.Data location)
        {
            return location != null ? new CommonModels.Location
            {
                LocationId = Convert.ToInt32(location.id),
                LocationCode = location.id.ToString(),
                LocationName = location.name + " " + location.name,
                IsPickup = true,
                Address = location.name + " " + location.name,
                PhoneNumber = "",
                MailAddress = ""
            } : null;
        }
    }
}
