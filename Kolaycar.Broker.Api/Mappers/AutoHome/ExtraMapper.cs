using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.AutoHome
{
    public static class ExtraMapper
    {
        public static Extra Map(this AutoHomeResponseBase.ListExtra extra, Vendor vendor) =>
      extra != null ? new Extra
      {
          VendorId = vendor.VendorId,
          VendorName = vendor.VendorName,
          ExtraId = 0,
          ExtraCode = extra.Id,
          ExtraType = AdditionalProductTypes.Extra,
          ExtraRentalType = ExtraRentalTypes.Daily,
          ExtraName = extra.Description,
          ApiExtraCode = extra.Id,
          Price = extra.Price.ToIntNullSafe(),
          ExtraDescription = extra.DescriptionLong,

      }
      : null;

        public static List<Extra> Map(this List<AutoHomeResponseBase.ListExtra> extras, Vendor vendor)
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
