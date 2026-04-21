using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.PandoraResponseBase;

namespace KolayCAR.Broker.API.Mappers.Pandora
{
    public static class ExtraMapper
    {
        public static Extra Map(this PandoraAddition extra) =>
            extra != null ? new Extra
            {
                ExtraId = extra.Id,
                ExtraCode = extra.Id.ToString(),
                ApiExtraCode = extra.Id.ToString(),
                ExtraName = extra.Name,
                ExtraRentalType = ExtraRentalTypes.PerRental,
                ExtraType = AdditionalProductTypes.Extra,
                ExtraQuantityIncreasable = false
            }
            : null;

        public static List<Extra> Map(this List<PandoraAddition> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }

        public static Extra Map(this AvailableService extra) =>
            extra != null ? new Extra
            {
                ExtraId = extra.ServiceId,
                ExtraCode = extra.ServiceId.ToString(),
                ApiExtraCode = extra.ServiceId.ToString(),
                ExtraName = extra.Name,
                ExtraDescription = extra.Description,
                ExtraRentalType = ExtraRentalTypes.PerRental,
                ExtraType = extra.ServiceTypeId == 2 ? AdditionalProductTypes.Extra : AdditionalProductTypes.Insurance,
                ExtraQuantityIncreasable = extra.MaximumQuantity != null,
                Price = extra.Amount.ToFloatNullSafe()
            }
            : null;

        public static List<Extra> Map(this List<AvailableService> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }
    }
}
