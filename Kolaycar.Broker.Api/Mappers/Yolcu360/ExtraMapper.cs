using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using Default = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Yolcu360
{
    public static class ExtraMapper
    {
        public static List<Default.Extra> Map(this List<Default.Response.Yolcu360ProductResponseBase.Data> extras, Default.Vehicle vehicle)
        {
            var _extras = new List<Default.Extra>();
            if (vehicle != null)
            {
                foreach (var extra in extras)
                {
                    _extras.Add(extra.Map(vehicle));
                }
            }

            return _extras;
        }
        public static Default.Extra Map(this Default.Response.Yolcu360ProductResponseBase.Data apiExtra, Default.Vehicle vehicle)
        {
            var price = (apiExtra.price.ToFloatNullSafe() / 100) / vehicle.RentalDuration;

            return apiExtra != null ? new Default.Extra
            {
                VendorName = vehicle.VendorName,
                VendorId = vehicle.VendorId,
                ExtraId = (int)Enum.Parse(typeof(Default.Response.YolcuProductsTypes), apiExtra.productType) + 1,
                ExtraCode = apiExtra.productType,
                ExtraRentalType = Default.ExtraRentalTypes.Daily,
                ExtraName = apiExtra.productType,
                ApiExtraCode = apiExtra.label,
                Label = apiExtra.label,
                DamageInsuranceCategory = apiExtra.damageInsuranceCategory ?? "",
                Price = price
            } : null;
        }
    }
}
