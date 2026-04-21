using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Mappers
{
    public static class SubVendorMapper
    {
        public static SubVendor Map(this Subvendor subVendor) =>
            subVendor != null ? new SubVendor
            {
                Id = subVendor.Id,
                SubVendorId = subVendor.Subvendorid,
                VendorId = subVendor.Vendorid,
                VendorName = subVendor.Vendorname,
                VendorType = (VendorTypes)subVendor.Vendortype,
                Active = subVendor.Active ?? false,
                ProfitMarkup = subVendor.Profitmarkup.ToFloatNullAvailable(),
                ProfitMarkupAdditionalProducts = subVendor.Profitmarkupadditionalproducts.ToFloatNullAvailable(),
                ProfitMarkupOneWayFee = subVendor.Profitmarkuponewayfee.ToFloatNullAvailable(),
                APIProfitMarkup = subVendor.Apiprofitmarkup.ToFloatNullAvailable(),
                APIProfitMarkupAdditionalProducts = subVendor.Apiprofitmarkupadditionalproducts.ToFloatNullAvailable(),
                APIProfitMarkupOneWayFee = subVendor.Apiprofitmarkuponewayfee.ToFloatNullAvailable(),
                Logo = subVendor.Logo,
                CurrencyId = subVendor.Currencyid ?? 1
            }
            : null;

        public static List<SubVendor> Map(this IEnumerable<Subvendor> subVendors)
        {
            var _subVendors = new List<SubVendor>();

            if (subVendors != null && subVendors.Count() != 0)
                foreach (var subVendor in subVendors)
                    _subVendors.Add(subVendor.Map());

            return _subVendors;
        }
    }
}
