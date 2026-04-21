using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Elitcar
{
    public static class ExtraMapper
    {
        public static Extra Map(this ElitcarResponseBase.ListExtra extra) =>
          extra != null ? new Extra
          {
              ExtraId = 0,
              ExtraCode = extra.id.ToStringNullSafe(),
              ApiExtraCode = extra.id.ToStringNullSafe(),
              ExtraName = extra.name,
              ExtraDescription = extra.description,
              ExtraRentalType = extra.price_type == "one_time" ? ExtraRentalTypes.PerRental : ExtraRentalTypes.Daily,
              ExtraQuantityIncreasable = extra.max_piece > 1,
              Price = extra.price.ToFloatNullSafe()
          }
          : null;

        public static Extra Map(this ElitcarResponseBase.Extra extra) =>
         extra != null ? new Extra
         {
             ExtraId = 0,
             ExtraCode = extra.id.ToStringNullSafe(),
             ApiExtraCode = extra.id.ToStringNullSafe(),
             ExtraName = extra.name + "-" + (extra.price_type == "one_time" ? "K" : "G"),
             ExtraDescription = extra.description,
             ExtraRentalType = extra.price_type == "one_time" ? ExtraRentalTypes.PerRental : ExtraRentalTypes.Daily,
             ExtraQuantityIncreasable = extra.max_piece > 1
         }
         : null;

        public static List<Extra> Map(this List<ElitcarResponseBase.ListExtra> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }

        public static List<Extra> Map(this List<ElitcarResponseBase.Extra> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }
    }
}
