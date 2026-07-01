using System;
using System.Collections.Generic;
using System.Linq;
using AdditionalProductTypes = KolayCAR.Broker.Domain.Models.AdditionalProductTypes;
using DomainExtra = KolayCAR.Broker.Domain.Models.Extra;
using ExtraRentalTypes = KolayCAR.Broker.Domain.Models.ExtraRentalTypes;
using PandoraExtra = KolayCAR.Broker.Domain.Models.Response.Pandora2ResponseBase.Extra;

namespace KolayCAR.Broker.API.Mappers.Pandora2
{
    public static class ExtraMapper
    {
        private const string PayNowPaymentType = "Now";

        public static DomainExtra Map(this PandoraExtra extra) =>
            extra != null ? new DomainExtra
            {
                ExtraId = Pandora2MapperHelper.ToStableId($"{extra.Id}-{extra.Group?.Id}"),
                ExtraCode = $"{extra.Id}<>{extra.Group?.Id}",
                ApiExtraCode = $"{extra.Id}",
                ExtraName = extra.Name,
                ExtraDescription = extra.Description,
                ExtraRentalType = ExtraRentalTypes.PerRental,
                ExtraType = extra.Group?.Id == 2 ? AdditionalProductTypes.Insurance : AdditionalProductTypes.Extra,
                ExtraQuantityIncreasable = extra.MaxQuantity != null && extra.MaxQuantity > 1,
                Price = Pandora2MapperHelper.ToMoney(extra.NetAmount),
                ApiPrice = Pandora2MapperHelper.ToMoney(extra.NetAmount),
                Icon = extra.ImgSrc,
                CurrencyCode = extra.Currency,
                Label = extra.Group?.Id.ToString()
            }
            : null;

        public static List<DomainExtra> Map(this List<PandoraExtra> extras)
        {
            var mappedExtras = new List<DomainExtra>();

            if (extras != null)
                foreach (var extra in extras)
                    mappedExtras.Add(extra.Map());

            return mappedExtras;
        }

        public static List<DomainExtra> Map(this List<PandoraExtra> extras, bool extraPricePayToDelivery) =>
            extras.FilterByPaymentType(extraPricePayToDelivery).Map();

        public static List<PandoraExtra> FilterByPaymentType(this List<PandoraExtra> extras, bool extraPricePayToDelivery)
        {
            if (extras == null)
                return new List<PandoraExtra>();

            return extras
                .Where(extra => ShouldKeepExtra(extra, extraPricePayToDelivery))
                .ToList();
        }

        private static bool ShouldKeepExtra(PandoraExtra extra, bool extraPricePayToDelivery)
        {
            var isPaymentNow = string.Equals(extra?.PaymentType?.Trim(), PayNowPaymentType, StringComparison.OrdinalIgnoreCase);

            return extraPricePayToDelivery ? !isPaymentNow : isPaymentNow;
        }
    }
}
