using KolayCAR.Broker.Domain.Models;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.Circular2ResponseBase;

namespace KolayCAR.Broker.API.Mappers.Circular2
{
    public static class ExtraMapper
    {
        public static Extra Map(this ExtraResponse extra, Vendor vendor) =>
       extra != null ? new Extra
       {
           VendorId = vendor.VendorId,
           VendorName = vendor.VendorName,
           ExtraId = extra.id,
           ExtraCode = extra.id.ToString(),
           ApiExtraCode = extra.id.ToString(),
           ExtraType = AdditionalProductTypes.Extra,
           ExtraName = extra.name,
           Price = extra.price,
           ExtraRentalType = GetExtraType(extra.priceType),
           ExtraDescription = extra.description,
           CurrencyCode = extra.currencycode
       }
       : null;

        public static List<Extra> Map(this List<ExtraResponse> extras, Vendor vendor)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map(vendor));

            return _extras;
        }
        private static ExtraRentalTypes GetExtraType(string extraType)
        {
            switch (extraType)
            {
                case "fixed/daily":
                    return ExtraRentalTypes.Daily;
                case "fixed/onetime":
                    return ExtraRentalTypes.PerRental;
                default:
                    return ExtraRentalTypes.Daily;
            }
        }
    }
}
