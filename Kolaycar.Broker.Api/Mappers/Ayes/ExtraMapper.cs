using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Ayes
{
    public static class ExtraMapper
    {
        public static Extra Map(this AyesExtra extra) =>
            extra != null ? new Extra
            {
                ExtraId = extra.id.ToIntNullSafe(),
                ExtraCode = extra.id,
                ApiExtraCode = extra.id,
                ExtraName = extra.baslik,
                ExtraDescription = extra.bilgi,
                ExtraRentalType = extra.tek_odeme == "1" ? ExtraRentalTypes.PerRental : ExtraRentalTypes.Daily,
                ExtraQuantityIncreasable = false,
                Price = extra.ucret.ToFloatNullSafe()
            }
            : null;

        public static List<Extra> Map(this List<AyesExtra> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }
    }
}
