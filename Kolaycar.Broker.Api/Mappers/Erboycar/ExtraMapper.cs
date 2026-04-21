using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Erboycar
{
    public static class ExtraMapper
    {
        public static Extra Map(this ErboycarResponseBase.ExtraResponse extra) =>
            extra != null ? new Extra
            {
                ExtraId = 0,
                ExtraCode = extra._id,
                ApiExtraCode = extra._id,
                ExtraName = extra.name_tr,
                ExtraDescription = extra.note.ToStringNullSafe(),
                ExtraRentalType = extra.vehicle_extra_category != null && !string.IsNullOrEmpty(extra.vehicle_extra_category.name) ?
                    extra.vehicle_extra_category.name.ToLower() == "günlük" ? ExtraRentalTypes.Daily : ExtraRentalTypes.PerRental : ExtraRentalTypes.PerRental,
                ExtraQuantityIncreasable = false,
                Price = extra.price_tl.ToFloatNullSafe()
            } : null;

        public static List<Extra> Map(this List<ErboycarResponseBase.ExtraResponse> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }
    }
}
