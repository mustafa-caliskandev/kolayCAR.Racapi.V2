using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Garenta
{
    public static class ExtraMapper
    {
        public static Extra Map(this GarentaResponseBase.EXTRA extra) =>
            extra != null ? new Extra
            {
                ExtraId = 0,
                ExtraCode = extra.PRODUCT_ID,
                ApiExtraCode = extra.PRODUCT_ID,
                ExtraName = extra.PROD_DESC_SHORT,
                ExtraDescription = extra.INFO_TEXT,
                ExtraRentalType = ExtraRentalTypes.PerRental,
                ExtraQuantityIncreasable = false,
                Price = extra.PRICE.ToFloatNullSafe()
            } : null;

        public static List<Extra> Map(this List<GarentaResponseBase.EXTRA> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }
    }
}
