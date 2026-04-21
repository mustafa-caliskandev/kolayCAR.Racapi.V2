using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Central
{
    public static class ExtraMapper
    {
        public static Extra Map(this CentralResponseBase.CentralAdditionalProduct extra) =>
            extra != null ? new Extra
            {
                ExtraId = !string.IsNullOrEmpty(extra.Id) ? Convert.ToInt32(extra.Id) : 0,
                ExtraName = extra.Description,
                ExtraCode = extra.GroupName,
                ApiExtraCode = extra.GroupName,
                ExtraRentalType = ExtraRentalTypes.Daily,
                Price = extra.Price.ToFloatNullSafe(),
                ExtraQuantityIncreasable = extra.GroupName == "cocuk_koltugu" || extra.GroupName == "bebek_koltugu"
            }
            : null;

        public static List<Extra> Map(this List<CentralResponseBase.CentralAdditionalProduct> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }
    }
}
