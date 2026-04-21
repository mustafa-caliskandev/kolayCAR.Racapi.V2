using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.KolayCARBroker
{
    public static class ExtraMapper
    {
        public static Extra Map(this Extra extra) =>
           extra != null ? new Extra
           {
               ExtraId = extra.ExtraId,
               ExtraCode = extra.ExtraId.ToStringNullSafe(),
               ApiExtraCode = extra.Code,
               ExtraName = extra.ExtraName,
               ExtraDescription = extra.ExtraDescription,
               ExtraRentalType = extra.ExtraRentalType,
               ExtraQuantityIncreasable = extra.ExtraQuantityIncreasable,
               Price = extra.Price.ToFloatNullSafe()
           }
           : null;

        public static List<Extra> Map(this List<Extra> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }

        public static ReservationExtra Map(this ReservationExtra extra) =>
           extra != null ? new ReservationExtra
           {
               ExtraId = extra.ExtraId,
               ExtraCode = extra.ExtraId.ToStringNullSafe(),
               ApiExtraCode = extra.ExtraId.ToStringNullSafe(),
               ExtraName = extra.ExtraName,
               ExtraDescription = extra.ExtraDescription,
               ExtraRentalType = extra.ExtraRentalType,
               ExtraQuantityIncreasable = extra.ExtraQuantityIncreasable,
               Price = extra.Price.ToFloatNullSafe(),
               //APIPrice = extra.APIPrice.ToFloatNullSafe()
               ApiPrice = extra.ApiPrice.ToFloatNullSafe()
           }
           : null;

        public static List<ReservationExtra> Map(this List<ReservationExtra> extras)
        {
            var _extras = new List<ReservationExtra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }

        public static Extra ToExtraMapper(ReservationExtra extra) =>
          extra != null ? new Extra
          {
              ExtraId = extra.ExtraId,
              ExtraCode = extra.ExtraId.ToStringNullSafe(),
              ApiExtraCode = extra.ExtraId.ToStringNullSafe(),
              ExtraName = extra.ExtraName,
              ExtraDescription = extra.ExtraDescription,
              ExtraRentalType = extra.ExtraRentalType,
              ExtraQuantityIncreasable = extra.ExtraQuantityIncreasable,
              Price = extra.Price.ToFloatNullSafe()
          }
          : null;

        public static List<Extra> ToExtraMapper(List<ReservationExtra> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(ToExtraMapper(extra));

            return _extras;
        }
    }
}
