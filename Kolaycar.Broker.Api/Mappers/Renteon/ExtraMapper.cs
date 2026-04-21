using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Renteon
{
    public static class ExtraMapper
    {
        public static Extra Map(this RenteonResponseBase.Service extra, Vendor vendor) =>
         extra != null ? new Extra
         {
             VendorName = vendor.VendorName,
             ExtraCode = extra.Code,
             ApiExtraCode = extra.Code,
             ExtraName = extra.Name,
         }
         : null;

        public static List<Extra> Map(this List<RenteonResponseBase.Service> extras, Vendor vendor)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map(vendor));

            return _extras;
        }
    }
}
