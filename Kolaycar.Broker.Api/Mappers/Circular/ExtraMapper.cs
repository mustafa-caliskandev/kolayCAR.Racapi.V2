using KolayCAR.Broker.Domain.Models;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.CircularResponseBase;

namespace KolayCAR.Broker.API.Mappers.Circular
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
             ExtraType = GetExtraType(extra.type),
             ExtraName = extra.name,
             Price = extra.price,
             ExtraRentalType = GetExtraRentalType(extra.priceType),
             ExtraDescription = extra.description,
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

        private static AdditionalProductTypes GetExtraType(string type)
        {
            switch (type)
            {
                case "insurance":
                    return AdditionalProductTypes.Insurance;
                case "product":
                    return AdditionalProductTypes.Extra;
                default:
                    return AdditionalProductTypes.Extra;
            }
        }

        private static ExtraRentalTypes GetExtraRentalType(string priceType)
        {
            switch (priceType)
            {
                case "fixed/daily":
                    return ExtraRentalTypes.Daily;
                case "fixed/onetime":
                    return ExtraRentalTypes.PerRental;
                default:
                    return ExtraRentalTypes.PerRental;
            }
        }
    }
}
