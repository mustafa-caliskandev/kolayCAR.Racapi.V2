using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Enuygun
{
    public static class ExtraMapper
    {

        public static Extra Map(this EnuygunResponse.Extra.ExtraService extra, Vendor vendor) =>
            extra != null ? new Extra
            {
                VendorId = vendor.VendorId,
                VendorName = vendor.VendorName,
                ExtraId = 0,
                ExtraName = extra.name,
                ExtraCode = extra.context,
                ApiExtraCode = extra.context,
                Price = extra.totalPrice.ToFloatNullSafe(),
                CurrencyCode = extra.currency,
                ExtraRentalType = ExtraRentalTypes.PerRental,


            }
            : null;

        public static List<Extra> Map(this List<EnuygunResponse.Extra.ExtraService> extras, Vendor vendor)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map(vendor));

            return _extras;
        }
    }
}
