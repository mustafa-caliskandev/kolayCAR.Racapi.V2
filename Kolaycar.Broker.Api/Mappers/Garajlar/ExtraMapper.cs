using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using System.Linq;
using static KolayCAR.Broker.Domain.Models.Response.GarajlarResponseBase;

namespace KolayCAR.Broker.API.Mappers.Garajlar
{
    public static class ExtraMapper
    {
        public static List<Extra> Map(this List<GarajlarExtraList> extras)
        {
            var _extras = new List<Extra>();

            if (extras.Count > 0)
            {   //2 kiralama başına 1 günlük
                _extras.AddRange(extras.Select(e => new Extra
                {
                    ExtraCode = e.code,
                    ApiExtraCode = e.code,
                    ExtraName = e.name,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Extra,
                    ExtraRentalType = ExtraRentalTypes.Daily
                }).ToList());
            }
            return _extras;
        }
    }
}
