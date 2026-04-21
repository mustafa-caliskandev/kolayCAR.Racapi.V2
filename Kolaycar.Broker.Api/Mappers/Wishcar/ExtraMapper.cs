using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Wishcar
{
    public static class ExtraMapper
    {
        public static Extra Map(this WishcarResponseBase.ExtraResponse extra) =>
        extra != null ? new Extra
        {

            ExtraId = extra.MASRAFKODU.ToIntNullSafe(),
            ExtraCode = extra.MASRAFKODU.ToStringNullSafe(),
            ApiExtraCode = extra.MASRAFKODU.ToStringNullSafe(),
            ExtraName = extra.EKMASRAF_TR,
            ExtraDescription = !string.IsNullOrEmpty(extra.ACIKLAMA_TR) ? extra.ACIKLAMA_TR : string.Empty,
            ExtraRentalType = extra.GUNLUK == "True" ? ExtraRentalTypes.PerRental : ExtraRentalTypes.PerRental,
            ExtraQuantityIncreasable = extra.DROPSAYISI.ToIntNullSafe() >= 2,
            Price = extra.SONFIYAT.ToFloatNullSafe()
        }
        : null;


        public static List<Extra> Map(this List<WishcarResponseBase.ExtraResponse> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }
    }
}
