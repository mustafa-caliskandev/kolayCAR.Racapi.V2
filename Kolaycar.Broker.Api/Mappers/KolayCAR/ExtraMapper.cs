using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.API.Helpers.KolayCAR;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using KolayCARResponse = KolayCAR.Broker.Domain.Models.Response;

namespace KolayCAR.Broker.API.Mappers.KolayCAR
{
    public static class ExtraMapper
    {
        public static Extra Map(this KolayCARResponse.EXTRA extra) =>
            extra != null ? new Extra
            {
                VendorId = extra.VENDORID,
                VendorName = extra.VENDORNAME,
                ExtraId = extra.EXTRAID,
                ExtraCode = extra.EXTRAID.ToString(),
                ApiExtraCode = extra.EXTRAID.ToString(),
                ExtraName = extra.EXTRANAME,
                ExtraDescription = extra.EXTRADESCRIPTION,
                ExtraRentalType = (ExtraRentalTypes)extra.EXTRAPERDAY,
                ExtraQuantityIncreasable = Convert.ToBoolean(extra.EXTRAQUANTITYINCREASABLE),
                Price = MoneyHelper.ToFloat(extra.PRICE)
            }
            : null;

        public static List<Extra> Map(this List<KolayCARResponse.EXTRA> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count != 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }

        public static ReservationExtra Map(this Reservationextra extra) =>
            extra != null ? new ReservationExtra
            {
                ExtraId = extra.Extraid,
                ExtraCode = extra.Productcode,
                ApiExtraCode = extra.Productcode,
                Piece = extra.Piece,
                ExtraName = extra.Extraname,
                Price = extra.Amount.ToFloatNullSafe(),
                ApiPrice = extra.Apiprice.ToFloatNullSafe(),
                //APIPrice = extra.Apiprice.ToFloatNullSafe(),
                ExtraRentalType = extra.Extrarentaltype != null ? (ExtraRentalTypes)extra.Extrarentaltype : (ExtraRentalTypes?)null,
                ExtraType = (extra.ExtraType != 0 && extra.ExtraType != null) ? (AdditionalProductTypes)extra.ExtraType : (AdditionalProductTypes?)null,
                ExtraDescription = extra.ExtraDescription.ToStringNullSafe(),
                AgencyAmount = extra.AgencyAmount.ToFloatNullSafe()
            }
            : null;

        public static List<ReservationExtra> Map(this List<Reservationextra> extras)
        {
            var _extras = new List<ReservationExtra>();

            if (extras != null && extras.Count != 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }
    }
}
