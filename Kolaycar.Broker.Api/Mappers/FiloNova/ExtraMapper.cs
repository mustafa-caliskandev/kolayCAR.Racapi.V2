using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.FiloNova
{
    public static class ExtraMapper
    {
        public static Extra Map(this FiloNovaResponseBase.AdditionalProduct extra) =>
           extra != null ? new Extra
           {
               ExtraId = 0,//product code dan product id ye geçilmesi istendi 14.03.2025
               ExtraCode = extra.productId,
               ApiExtraCode = extra.productId,
               ExtraName = extra.productName,
               ExtraDescription = extra.productDescription,
               ExtraRentalType = extra.priceCalculationType switch { 1 => ExtraRentalTypes.Daily, 2 => ExtraRentalTypes.PerRental, _ => ExtraRentalTypes.Daily },
               ExtraQuantityIncreasable = extra.maxPieces > 1,
               Price = extra.priceCalculationType switch { 1 => extra.dailyAmount.ToFloatNullSafe(), 2 => extra.actualAmount.ToFloatNullSafe(), _ => extra.actualAmount.ToFloatNullSafe() }
           }
           : null;

        public static List<Extra> Map(this List<FiloNovaResponseBase.AdditionalProduct> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }
    }
}
