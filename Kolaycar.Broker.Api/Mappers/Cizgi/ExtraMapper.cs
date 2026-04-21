using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.CizgiResponseBase;

namespace KolayCAR.Broker.API.Mappers.Cizgi
{
    public static class ExtraMapper
    {
        public static Extra Map(this AvaibilityExtraResponse extra) =>
          extra != null ? new Extra
          {
              ExtraId = extra.id,
              ExtraCode = extra.eqp.ToStringNullSafe(),
              ApiExtraCode = extra.eqp.ToStringNullSafe(),
              ExtraName = extra.equipment,
              ExtraDescription = "",
              ExtraRentalType = ExtraRentalTypes.PerRental,
              ExtraQuantityIncreasable = extra.max_amount >= 3,
              Price = extra.for_duration.ToFloatNullSafe()
          }
          : null;

        public static Extra Map(this ExtraList extra) =>
         extra != null ? new Extra
         {

             ExtraId = extra.id,
             ExtraCode = extra.eqp.ToStringNullSafe(),
             ApiExtraCode = extra.eqp.ToStringNullSafe(),
             ExtraName = extra.equipment,
             ExtraDescription = string.Empty,
             ExtraRentalType = ExtraRentalTypes.PerRental,
             ExtraQuantityIncreasable = extra.max_amount >= 3
         }
         : null;

        public static List<Extra> Map(this List<AvaibilityExtraResponse> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }

        public static List<Extra> Map(this List<ExtraList> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }
    }
}
