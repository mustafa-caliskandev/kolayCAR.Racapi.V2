using KolayCAR.Broker.Domain.Models.Sixt.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using System.Linq;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Sixt
{
    public static class ExtraMapper
    {
        public static List<CommonModels.Extra> Map(this VEHICLE apiVehicle)
        {
            var _extras = new List<CommonModels.Extra>();

            if (apiVehicle.EXTRAS.EXTRA.Count > 0)
            {
                _extras.AddRange(apiVehicle.EXTRAS.EXTRA.Select(x => new CommonModels.Extra
                {
                    ExtraId = apiVehicle.EXTRAS.EXTRA.IndexOf(x), // 
                    ExtraCode = x.CODE,
                    ApiExtraCode = x.CODE,
                    ExtraName = x.NAME,
                    ExtraRentalType = x.CALCULATEINFO.INFO == "Günlük" && x.CALCULATEINFO.STATUS == "0" ? CommonModels.ExtraRentalTypes.Daily : CommonModels.ExtraRentalTypes.PerRental,
                    ExtraQuantityIncreasable = false,
                    Price = x.PRICE.ToFloatNullSafe(),
                    ExtraType = CommonModels.AdditionalProductTypes.Extra
                }).ToList());
            }

            if (apiVehicle.INSURANCES.INSURANCE.Count > 0)
            {
                _extras.AddRange(apiVehicle.INSURANCES.INSURANCE.Select(x => new CommonModels.Extra
                {
                    ExtraId = apiVehicle.INSURANCES.INSURANCE.IndexOf(x), // 
                    ExtraCode = x.CODE,
                    ApiExtraCode = x.CODE,
                    ExtraName = x.NAME,
                    ExtraRentalType = x.CALCULATEINFO.INFO == "Günlük" && x.CALCULATEINFO.STATUS == "0" ? CommonModels.ExtraRentalTypes.Daily : CommonModels.ExtraRentalTypes.PerRental,
                    ExtraQuantityIncreasable = false,
                    Price = x.PRICE.ToFloatNullSafe(),
                    ExtraType = CommonModels.AdditionalProductTypes.Insurance
                }).ToList());
            }

            if (apiVehicle.INCLUDED != null)
            {
                if (apiVehicle.INCLUDED.INCLUDE.Count > 0)
                {
                    apiVehicle.INCLUDED.INCLUDE.RemoveAll(x => x == null);
                    _extras.AddRange(apiVehicle.INCLUDED.INCLUDE.Select(x => new CommonModels.Extra
                    {
                        ExtraId = apiVehicle.INCLUDED.INCLUDE.IndexOf(x),
                        ExtraCode = x.CODE,
                        ApiExtraCode = x.CODE,
                        ExtraName = x.NAME,
                        ExtraRentalType = CommonModels.ExtraRentalTypes.PerRental,
                        ExtraType = CommonModels.AdditionalProductTypes.InternalService
                    }).ToList());
                }

            }

            return _extras;
        }
    }
}
