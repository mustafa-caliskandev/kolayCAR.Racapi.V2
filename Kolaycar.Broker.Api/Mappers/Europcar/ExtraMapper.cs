using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Europcar
{
    public static class ExtraMapper
    {
        public static Extra Map(this option extra) =>
            extra != null ? new Extra
            {
                ExtraId = 0,
                ExtraCode = extra.code,
                ApiExtraCode = extra.code,
                ExtraName = extra.name,
                ExtraRentalType = ExtraRentalTypes.PerRental,
                Price = VehicleHelper.ConvertCommaFreePriceToFloat(extra.rate)
            }
            : null;

        public static List<Extra> Map(this List<option> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }

        public static Extra Map(this optionList extra) =>
            extra != null ? new Extra
            {
                ExtraId = 0,
                ExtraCode = extra.code,
                ApiExtraCode = extra.code,
                ExtraName = extra.name,
                ExtraRentalType = ExtraRentalTypes.PerRental,
                Price = VehicleHelper.ConvertCommaFreePriceToFloat(extra.rate)
            }
            : null;
        public static List<Extra> Map(this List<optionList> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }
        public static Extra Map(this EuropcarExtra extra) =>
        extra != null ? new Extra
        {
            ExtraId = 0,
            ExtraCode = extra.serviceCode,
            ApiExtraCode = extra.serviceCode,
            ExtraName = extra.serviceName,
            ExtraRentalType = ExtraRentalTypes.PerRental,
            ExtraQuantityIncreasable = extra.isRentableMoreThanOne
        }
        : null;

        public static List<Extra> Map(this List<EuropcarExtra> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }
    }
}
