using KolayCAR.Broker.Domain.Models.Response;
using System;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Central2
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this Central2ResponseBase.Location location) =>
            location != null ? new CommonModels.Location
            {
                LocationId = Convert.ToInt32(location.Id),
                LocationCode = location.Location_ID.ToString(),
                LocationName = location.Location_Name,
                IsPickup = true,
                Address = location.Address,
                PhoneNumber = location.Telephone,
                MailAddress = location.Mail_Adress,
                IsOffice = location.Delivery_Type == "Ofis Teslim"
            }
            : null;

        public static List<CommonModels.Location> Map(this List<Central2ResponseBase.Location> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
