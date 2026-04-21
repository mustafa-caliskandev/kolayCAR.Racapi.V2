using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Ototur
{
    public static class ExtraMapper
    {
        public static Extra Map(this OtoturResponseBase.Extra extra, Vendor vendor) =>
     extra != null ? new Extra
     {
         VendorId = vendor.VendorId,
         VendorName = vendor.VendorName,
         ExtraId = int.Parse(extra.id),
         ExtraCode = extra.id.ToStringNullSafe(),
         ApiExtraCode = extra.id.ToStringNullSafe(),
         ExtraType = AdditionalProductTypes.Extra,
         ExtraName = extra.name,
         ExtraRentalType = extra.sellType == "PER_DAY" ? ExtraRentalTypes.Daily : extra.sellType == "PER_RENTAL" ? ExtraRentalTypes.PerRental : ExtraRentalTypes.Daily,

     }
     : null;

        public static List<Extra> Map(this List<OtoturResponseBase.Extra> extras, Vendor vendor)
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
