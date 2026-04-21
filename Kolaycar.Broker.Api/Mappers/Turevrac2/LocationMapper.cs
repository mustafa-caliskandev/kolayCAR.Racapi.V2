using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Turevrac2
{
    public static class LocationMapper
    {
        public static Domain.Models.Location Map(this Turevrac2ResponseBase.Location location) =>
            location != null
                    ? new Domain.Models.Location
                    {
                        LocationId = location.location_id.ToIntNullSafe(),
                        LocationCode = location.location_id.ToStringNullSafe(),
                        LocationName = location.location_name,
                        IsPickup = true,
                        Address = location.address,
                        PhoneNumber = location.telephone,
                        MailAddress = location.mail_adress
                    }
                    : null;
        public static List<Domain.Models.Location> Map(this List<Turevrac2ResponseBase.Location> list)
        {
            var locationList = new List<Domain.Models.Location>();
            if (list != null)
                foreach (var location in list)
                {
                    locationList.Add(location.Map());
                }
            return locationList;
        }
    }
}
