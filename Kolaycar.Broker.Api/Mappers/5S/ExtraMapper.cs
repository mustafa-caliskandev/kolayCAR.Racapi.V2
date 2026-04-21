using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers._5S
{
    public static class ExtraMapper
    {
        public static List<CommonModels.Extra> Map(this BesSResponseBase.ExtraResponse extraResponse)
        {
            var _extras = new List<CommonModels.Extra>();
            var extras = extraResponse.data;

            foreach (var extra in extras)
            {
                _extras.Add(Map(extra));
            }

            return _extras;
        }

        private static CommonModels.Extra Map(BesSResponseBase.Data extra)
        {
            return new CommonModels.Extra
            {
                ExtraId = extra.id.ToIntNullSafe(),
                ExtraCode = extra.id.ToString(),
                ApiExtraCode = extra.id.ToString(),
                ExtraName = extra.name.tr,
                ExtraDescription = extra.description.tr,
                ExtraRentalType = extra.extra_type == 0 ? CommonModels.ExtraRentalTypes.Daily : CommonModels.ExtraRentalTypes.PerRental,
                ExtraQuantityIncreasable = extra.multiplying_days > 1,
                Price = extra.fixed_price.ToFloatNullSafe()
            };
        }

        public static List<CommonModels.Extra> Map(this List<BesSResponseBase.ExtraSearchResponse> extraSearchResponse)
        {
            var _extras = new List<CommonModels.Extra>();
            var extras = extraSearchResponse;

            foreach (var extra in extras)
            {
                _extras.Add(Map(extra));
            }

            return _extras;
        }
        private static CommonModels.Extra Map(BesSResponseBase.ExtraSearchResponse extra)
        {
            return new CommonModels.Extra
            {
                ExtraId = extra.id,
                ExtraCode = extra.id.ToString(),
                ApiExtraCode = extra.id.ToString(),
                ExtraName = extra.name.tr,
                ExtraDescription = extra.description.tr,
                Price = extra.extra_type == 0 ? extra.daily_price.ToFloatNullSafe() : extra.price.ToFloatNullSafe(),
                ExtraRentalType = extra.extra_type == 0 ? CommonModels.ExtraRentalTypes.Daily : CommonModels.ExtraRentalTypes.PerRental
            };
        }
    }
}
