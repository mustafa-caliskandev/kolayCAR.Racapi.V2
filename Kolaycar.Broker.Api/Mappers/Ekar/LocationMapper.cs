using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Ekar
{
    public static class LocationMapper
    {
        public static CommonModels.Location Map(this EkarResponseBase.Sube location) =>
            location != null ? new CommonModels.Location
            {
                LocationId = location.Kayit_ID.ToIntNullSafe(),
                LocationCode = location.Sube_Kodu,
                Address = location.Adres,
                LocationName = location.Sehir,
                MailAddress = location.Mail_Adresi,
                PhoneNumber = location.Telefon
            }
            : null;

        public static List<CommonModels.Location> Map(this List<EkarResponseBase.Sube> locations)
        {
            var _locations = new List<CommonModels.Location>();

            if (locations != null && locations.Count != 0)
                foreach (var location in locations)
                    _locations.Add(location.Map());

            return _locations;
        }
    }
}
