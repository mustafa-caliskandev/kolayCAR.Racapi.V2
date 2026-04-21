using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Mappers.Sixt2
{
    public static class ExtraMapper
    {
        public static List<Extra> Map(this List<SixtExtra> extras)
        {
            var _extras = new List<Extra>();

            if (extras.Count > 0)
            {
                _extras.AddRange(extras.Select(e => new Extra
                {
                    ExtraId = extras.IndexOf(e), 
                    ExtraCode = e.code,
                    ApiExtraCode = e.code,
                    ExtraName = e.name,
                    ExtraRentalType = e.calculation_type == 1 ? ExtraRentalTypes.Daily : ExtraRentalTypes.PerRental,
                    ExtraQuantityIncreasable = false,
                    Price = e.price.ToFloatNullSafe(),
                    ExtraType = AdditionalProductTypes.Extra
                }).ToList());
            }
            return _extras;
        }
        public static List<Extra> Map(this List<SixtExtraList> extras)
        {
            var _extras = new List<Extra>();

            if (extras.Count > 0)
            {   //2 kiralama başına 1 günlük
                _extras.AddRange(extras.Select(e => new Extra
                {
                    ExtraId = extras.IndexOf(e),
                    ExtraCode = e.code,
                    ApiExtraCode = e.code,
                    ExtraName = e.name,
                    ExtraRentalType = e.calculation_type == 1 ? ExtraRentalTypes.Daily : ExtraRentalTypes.PerRental,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Extra
                }).ToList());
            }
            return _extras;
        }
    }
}
