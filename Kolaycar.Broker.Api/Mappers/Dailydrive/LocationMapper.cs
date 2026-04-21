using KolayCAR.Broker.Domain.Models.Response.Dailydrive;
using System;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Dailydrive
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this Ns2Locations location) =>
            location != null ? new CommonModels.Location
            {
                LocationId = Convert.ToInt32(location.Ns2LocNo),
                LocationCode = location.Ns2LocNo.ToString(),
                LocationName = location.Ns2LocName,
                IsPickup = true,
                Address = location.Ns2LocAddress,
                PhoneNumber = location.Ns2LocPhone.ToString(),
                MailAddress = location.Ns2LocEmail
            }
            : null;

        public static List<CommonModels.Location> Map(this List<Ns2Locations> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
