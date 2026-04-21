using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.GreenMotion
{
    public static class ExtraMapper
    {
        public static Extra Map(this GreenMotionOption extra) =>
            extra != null ? new Extra
            {
                ExtraId = extra.optionID.ToIntNullSafe(),
                ExtraCode = extra.optionID,
                ApiExtraCode = extra.optionID,
                ExtraName = extra.Name,
                ExtraDescription = extra.Description,
                ExtraType = AdditionalProductTypes.Insurance,
                ExtraRentalType = ExtraRentalTypes.PerRental,
                ExtraQuantityIncreasable = false,
                Price = extra.Total_for_this_booking.ToFloatNullSafe()
            }
            : null;

        public static Extra Map(this GreenMotionExtra extra) =>
            extra != null ? new Extra
            {
                ExtraId = extra.optionID.ToIntNullSafe(),
                ExtraCode = extra.optionID,
                ApiExtraCode = extra.optionID,
                ExtraName = extra.Name,
                ExtraDescription = extra.Description,
                ExtraType = extra.Category.TrimNullSafe().ToLower().Contains("additional") ? AdditionalProductTypes.Extra : AdditionalProductTypes.Insurance,
                ExtraRentalType = ExtraRentalTypes.PerRental,
                ExtraQuantityIncreasable = false,
                Price = extra.Total_for_this_booking.__text.ToFloatNullSafe()
            }
            : null;

        public static List<Extra> Map(this GreenMotionInsuranceOptions extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.option.Count > 0)
                foreach (var extra in extras.option)
                    if (extra != null)
                        _extras.Add(extra.Map());

            return _extras;
        }

        public static List<Extra> Map(this GreenMotionOptionalextras extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Extras.Count > 0)
                foreach (var extra in extras.Extras)
                    _extras.Add(extra.Map());

            return _extras;
        }
    }
}
