using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response.Vonarent;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Vonarent
{
    public static class ExtraMapper
    {
        public static List<Extra> Map(this List<VonarentExtraItem> items)
        {
            var extras = new List<Extra>();
            if (items == null)
                return extras;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                extras.Add(new Extra
                {
                    ExtraId = i + 1,
                    ExtraCode = item.id,
                    ApiExtraCode = item.id,
                    ExtraName = item.name.ToStringNullSafe(),
                    ExtraDescription = item.description.ToStringNullSafe(),
                    ExtraType = MapAdditionalProductType(item.extraCategoryName),
                    ExtraRentalType = MapExtraRentalType(item.priceCalculationType),
                    ExtraQuantityIncreasable = item.isMultipleSelectable == 1,
                    Price = item.price,
                    ApiPrice = item.price,
                    DefaultPrice = item.price,
                    CurrencyCode = item.currency.ToStringNullSafe(),
                    CurrencyType = GetCurrencyType(item.currency),
                    VendorExtraExists = true,
                    Label = item.extraCategoryName.ToStringNullSafe(),
                    Piece = 1
                });
            }

            return extras;
        }

        private static CurrencyTypes? GetCurrencyType(string currencyCode)
            => Enum.TryParse(currencyCode, true, out CurrencyTypes currencyType) ? currencyType : null;

        private static ExtraRentalTypes MapExtraRentalType(string priceCalculationType)
            => priceCalculationType.ToStringNullSafe().Trim().ToUpperInvariant() == "DAILY"
                ? ExtraRentalTypes.Daily
                : ExtraRentalTypes.PerRental;

        private static AdditionalProductTypes MapAdditionalProductType(string categoryName)
        {
            var normalized = categoryName.ToStringNullSafe().Trim().ToLowerInvariant();
            if (normalized.Contains("insur") || normalized.Contains("sigorta") || normalized.Contains("cover"))
                return AdditionalProductTypes.Insurance;
            if (normalized.Contains("premium"))
                return AdditionalProductTypes.Premium;
            if (normalized.Contains("mandatory") || normalized.Contains("zorunlu"))
                return AdditionalProductTypes.Compulsory;
            return AdditionalProductTypes.Extra;
        }
    }
}
