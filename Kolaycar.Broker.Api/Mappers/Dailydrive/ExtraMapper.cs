using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response.Dailydrive;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Dailydrive
{
    public static class ExtraMapper
    {
        public static Extra Map(this Ns2ExtraProducts extra) =>
            extra != null ? new Extra
            {
                ExtraId = extra.Ns2ProductNo.ToIntNullSafe(),
                ExtraCode = extra.Ns2ProductNo,
                ApiExtraCode = extra.Ns2ProductNo,
                ExtraName = extra.Ns2ProductName,
                ExtraDescription = extra.Ns2ProductDescription,
                ExtraRentalType = ExtraRentalTypes.PerRental,
                ExtraQuantityIncreasable = false,
                Price = extra.Ns2TotalPrice.ToFloatNullSafe()
            }
            : null;

        public static List<Extra> Map(this List<Ns2ExtraProducts> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }
    }
}
