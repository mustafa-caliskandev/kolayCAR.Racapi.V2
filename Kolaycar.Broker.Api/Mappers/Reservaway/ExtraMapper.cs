using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response.Reservaway;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Reservaway
{
    public static class ExtraMapper
    {
        public static List<Extra> Map(this List<ReservawayVehicleExtra> items)
        {
            var extras = new List<Extra>();
            if (items == null)
                return extras;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                extras.Add(new Extra
                {
                    ExtraId = item.extra_id > 0 ? item.extra_id : i + 1,
                    ExtraCode = item.extra_id > 0 ? item.extra_id.ToString() : item.extra_code,
                    ApiExtraCode = item.extra_code.ToStringNullSafe(),
                    ExtraName = item.extra_name.ToStringNullSafe(),
                    ExtraDescription = item.extra_description.ToStringNullSafe(),
                    ExtraType = MapAdditionalProductType(item),
                    ExtraRentalType = item.extra_rental_type == 1 ? ExtraRentalTypes.PerRental : ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = item.extra_quantity_increasable,
                    Price = item.price,
                    ApiPrice = item.price,
                    DefaultPrice = item.price,
                    Icon = string.Empty,
                    CurrencyCode = item.currency_code.ToStringNullSafe(),
                    CurrencyType = GetCurrencyType(item.currency_code),
                    VendorExtraExists = true,
                    Label = item.label.ToStringNullSafe(),
                    DamageInsuranceCategory = item.damage_insurance_category.ToStringNullSafe(),
                    Piece = 1
                });
            }

            return extras;
        }

        private static CurrencyTypes? GetCurrencyType(string currencyCode)
            => Enum.TryParse(currencyCode, true, out CurrencyTypes currencyType) ? currencyType : null;

        private static AdditionalProductTypes MapAdditionalProductType(ReservawayVehicleExtra item)
        {
            var normalized = $"{item.extra_name} {item.label} {item.damage_insurance_category}".ToStringNullSafe().ToLowerInvariant();
            if (normalized.Contains("insur") || normalized.Contains("cover") || normalized.Contains("protection"))
                return AdditionalProductTypes.Insurance;

            return AdditionalProductTypes.Extra;
        }
    }
}
