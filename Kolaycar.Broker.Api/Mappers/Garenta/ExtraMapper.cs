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
                ExtraName = !string.IsNullOrWhiteSpace(extra.PROD_DESC_SHORT) ? extra.PROD_DESC_SHORT : extra.PROD_DESC_LARGE,
                ExtraDescription = !string.IsNullOrWhiteSpace(extra.INFO_TEXT) ? extra.INFO_TEXT : extra.PROD_DESC_LARGE,
                ExtraRentalType = ExtraRentalTypes.PerRental,
                ExtraQuantityIncreasable = false,
                Price = GetPrice(extra),
                ApiPrice = GetPrice(extra),
                CurrencyCode = extra.CURRENCY,
                IsRequired = extra.MANDATORY == "X"
            } : null;

        public static List<Extra> Map(this List<GarentaResponseBase.EXTRA> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }

        private static float GetPrice(GarentaResponseBase.EXTRA extra)
        {
            var netAmount = extra.NET_AMOUNT.ToStringNullSafe();

            if (!string.IsNullOrWhiteSpace(netAmount))
                return netAmount.ToFloatNullSafe();

            return extra.PRICE.ToStringNullSafe().ToFloatNullSafe();
        }
    }
}
