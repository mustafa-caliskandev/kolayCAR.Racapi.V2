using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers
{
    public static class VehicleResultStatisticMapper
    {
        public static CommonModels.VehicleResultStatistic Map(this Vehicleresultstatistic vehicleResultStatistic) =>
            vehicleResultStatistic != null ? new CommonModels.VehicleResultStatistic
            {
                Id = vehicleResultStatistic.Id,
                RecordDate = vehicleResultStatistic.Recorddate,
                PickupLocationId = vehicleResultStatistic.Pickuplocationid,
                ReturnLocationId = vehicleResultStatistic.Returnlocationid,
                VendorId = vehicleResultStatistic.Vendorid,
                VendorName = vehicleResultStatistic.Vendorname,
                APIVendorId = vehicleResultStatistic.Apivendorid,
                APIVendorName = vehicleResultStatistic.Apivendorname,
                VehicleId = vehicleResultStatistic.Vehicleid,
                VehicleCode = vehicleResultStatistic.Vehiclecode,
                VehicleName = vehicleResultStatistic.Vehiclename,
                DailyPrice = vehicleResultStatistic.Dailyprice.ToFloatNullSafe(),
                RentalDuration = vehicleResultStatistic.Rentalduration,
                VehicleImageUrl = vehicleResultStatistic.Vehicleimageurl,
                AgencyId = vehicleResultStatistic.Agencyid,
                VendorLogoUrl = vehicleResultStatistic.Vendorlogourl,
                FuelId = vehicleResultStatistic.Fuelid,
                TransmissionId = vehicleResultStatistic.Transmissionid,
                BaggageId = vehicleResultStatistic.Baggageid,
                CategoryId = vehicleResultStatistic.Categoryid,
                PersonId = vehicleResultStatistic.Personid,
                TypeId = vehicleResultStatistic.Typeid,
                PickupDate = vehicleResultStatistic.Pickupdate,
                ReturnDate = vehicleResultStatistic.Returndate,
                CurrencyId = vehicleResultStatistic.Currencyid,
                LangId = vehicleResultStatistic.Langid,
                ExchangeRate = vehicleResultStatistic.Exchangerate.ToFloatNullSafe()
            }
            : null;

        public static List<CommonModels.VehicleResultStatistic> Map(this List<Vehicleresultstatistic> vehicleResultStatistics)
        {
            var _vehicleResultStatistic = new List<CommonModels.VehicleResultStatistic>();

            if (vehicleResultStatistics != null && vehicleResultStatistics.Count != 0)
                foreach (var vehicleResultStatistic in vehicleResultStatistics)
                    _vehicleResultStatistic.Add(vehicleResultStatistic.Map());

            return _vehicleResultStatistic;
        }
    }
}
