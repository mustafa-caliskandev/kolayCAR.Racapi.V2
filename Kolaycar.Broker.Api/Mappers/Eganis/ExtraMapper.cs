using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Eganis
{
    public static class ExtraMapper
    {
        public static Extra Map(this EganisResponseBase.ExtraResponse.Extra extra, Vendor vendor) =>
             extra != null ? new Extra
             {
                 VendorId = vendor.VendorId,
                 VendorName = vendor.VendorName,
                 ExtraCode = extra.code,
                 ApiExtraCode = extra.code,
                 ExtraName = extra.name,
                 ExtraDescription = extra.name,
                 ExtraRentalType = (ExtraRentalTypes)extra.rentalType
             }
 : null;

        public static List<Extra> Map(this List<EganisResponseBase.ExtraResponse.Extra> extras, Vendor vendor)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map(vendor));

            return _extras;
        }

        public static List<Extra> Map(this List<EganisResponseBase.VehicleResponse.Extras> extras, Vendor vendor)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map(vendor));

            return _extras;
        }

        public static Extra Map(this EganisResponseBase.VehicleResponse.Extras extra, Vendor vendor) =>
     extra != null ? new Extra
     {
         VendorId = vendor.VendorId,
         VendorName = vendor.VendorName,
         ExtraCode = extra.code,
         ExtraName = extra.name,
         ExtraDescription = extra.name,
         ExtraRentalType = ExtraRentalTypes.PerRental,
         Price = extra.fee,
         ExtraType = AdditionalProductTypes.Extra,
         ApiExtraCode = extra.code
         
     }
: null;


    }
}
