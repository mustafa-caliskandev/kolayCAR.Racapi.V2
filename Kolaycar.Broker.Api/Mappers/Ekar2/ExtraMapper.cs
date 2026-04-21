using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Ekar2
{
    public static class ExtraMapper
    {
        public static Extra Map(this Ekar2ResponseBase.Extra extra) =>
           extra != null ? new Extra
           {
               ExtraId = extra.id,
               ExtraCode = extra.id.ToStringNullSafe(),
               ApiExtraCode = extra.id.ToStringNullSafe(),
               ExtraName = extra.name,
               ExtraRentalType = extra.sellType == "PER_DAY" ? ExtraRentalTypes.Daily : ExtraRentalTypes.PerRental,
               ExtraQuantityIncreasable = false,
               Price = extra.totalPrice.ToFloatNullSafe()
           }
       : null;


        public static List<Extra> Map(this Ekar2ResponseBase.Ekar2ExtraListResult extrasResult)
        {
            var _extras = new List<Extra>();
            var extras = extrasResult.content;
            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }
    }
}
