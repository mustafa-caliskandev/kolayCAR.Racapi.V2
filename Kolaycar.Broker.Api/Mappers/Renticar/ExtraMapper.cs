using KolayCAR.Broker.API.Helpers.Renticar;
using KolayCAR.Broker.Domain.Models;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Renticar
{
    public static class ExtraMapper
    {
        public static List<Extra> Map(this List<Domain.Models.Renticar.Response.Extra> apiExtras, CurrencyTypes currency)
        {
            var _extras = new List<Extra>();

            foreach (var item in apiExtras)
                _extras.Add(item.Map(currency));

            return _extras;
        }

        private static Extra Map(this Domain.Models.Renticar.Response.Extra apiExtra, CurrencyTypes currency) =>
            apiExtra != null ? new Extra
            {
                ExtraId = 0,
                ExtraCode = apiExtra.name,
                ApiExtraCode = apiExtra.name,
                ExtraName = apiExtra.name,
                ExtraDescription = apiExtra.description,
                ExtraRentalType = apiExtra.extratype == 1 ? ExtraRentalTypes.PerRental : ExtraRentalTypes.Daily,
                ExtraQuantityIncreasable = false,
                Price = RenticarHelper.GetExtraPrice(apiExtra, currency, apiExtra.extratype == 1 ? 2 : 1)
            } : null;
    }
}
