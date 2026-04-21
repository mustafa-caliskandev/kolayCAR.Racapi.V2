using System.Collections.Generic;
using KolayCAR.Broker.Domain.Models;
using static KolayCAR.Broker.Domain.Models.Response.ZiraatFiloResposeBase;

namespace KolayCAR.Broker.API.Mappers.ZiraatFilo
{
    public static class ExtraMapper
    {
        public static Extra Map(this MesajBilgi extra, Vendor vendor) =>
         extra != null ? new Extra
         {
             VendorName = vendor.VendorName,
             ExtraCode = extra.Kod,
             ApiExtraCode = extra.Kod,
             ExtraName = extra.Subject,
             ExtraDescription = extra.Desc,
             ExtraRentalType = ExtraRentalTypes.Daily,
             ExtraType = AdditionalProductTypes.Extra
         }
         : null;

        public static List<Extra> Map(this List<MesajBilgi> extras, Vendor vendor)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map(vendor));

            return _extras;
        }
    }
}
