using KolayCAR.Broker.Domain.Models;
using System;
using System.Collections.Generic;
using KolayCARResponse = KolayCAR.Broker.Domain.Models.Response;

namespace KolayCAR.Broker.Domain.Mappers.KolayCAR
{
    public static class ExtraMapper
    {
        public static Extra Map(this KolayCARResponse.EXTRA extra) =>
            extra != null ? new Extra
            {
                ExtraId = extra.EXTRAID,
                ExtraName = extra.EXTRANAME,
                ExtraDescription = extra.EXTRADESCRIPTION,
                ExtraRentalType = (ExtraRentalTypes)extra.EXTRAPERDAY,
                ExtraQuantityIncreasable = Convert.ToBoolean(extra.EXTRAQUANTITYINCREASABLE),
                Price = extra.PRICE
            }
            : null;

        public static List<Extra> Map(this List<KolayCARResponse.EXTRA> extras)
        {
            var _extras = new List<Extra>();

            if (extras.Count != 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }
    }
}
