using KolayCAR.Broker.Domain.Models.Response;
using System;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Turmobil
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this TurmobilResponseBase.Location location) =>
            location != null ? new CommonModels.Location
            {
                LocationId = Convert.ToInt32(location.id),
                LocationCode = location.id.ToString(),
                LocationName = location.name,
                IsPickup = true,
                Address = location.address,
                PhoneNumber = location.phone,
                MailAddress = location.email
            }
            : null;

        public static List<CommonModels.Location> Map(this List<TurmobilResponseBase.Location> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
