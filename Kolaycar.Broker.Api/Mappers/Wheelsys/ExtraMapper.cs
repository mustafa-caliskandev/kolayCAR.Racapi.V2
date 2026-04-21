using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.WheelsysResponseBase;

namespace KolayCAR.Broker.API.Mappers.Wheelsys
{
    public static class ExtraMapper
    {
        public static Extra Map(this Option extra, Vendor vendor) =>
                extra != null ? new Extra
                {
                    VendorId = vendor.VendorId,
                    VendorName = vendor.VendorName,
                    ExtraId = 0,
                    ExtraCode = extra.code.ToStringNullSafe(),
                    ApiExtraCode = extra.code.ToStringNullSafe(),
                    ExtraType = extra.quant == "true" ? AdditionalProductTypes.Extra : AdditionalProductTypes.Insurance,
                    ExtraName = extra.name,
                    ExtraRentalType = extra.quant == "true" ? ExtraRentalTypes.Daily : ExtraRentalTypes.PerRental,
                    Price = VehicleHelper.ConvertCommaFreePriceToFloat(extra.rate)

                }
                : null;

        public static List<Extra> Map(this List<Option> extras, Vendor vendor)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map(vendor));

            return _extras;
        }
    }
}
