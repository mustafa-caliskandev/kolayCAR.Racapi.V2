using System.Collections.Generic;

namespace Kolaycar.Broker.Api.Mappers.YesOto
{
    public static class VehicleMapper
    {
        public static KolayCAR.Broker.Domain.Models.Vehicle Map(this KolayCAR.Broker.Domain.Models.Responses.YesOto.YesOtoVehicleData vehicle)
        {
            if (vehicle == null) return null;

            var result = new KolayCAR.Broker.Domain.Models.Vehicle
            {
                VehicleId = 0,
                VehicleCode = vehicle.id,
                VehicleName = vehicle.vehicleGroup?.name ?? "",
                VehicleDescription = "",
                DailyPrice = vehicle.totalDiscountedPrice / (vehicle.rentalDayCount > 0 ? vehicle.rentalDayCount : 1),
                TotalPrice = vehicle.discountedGrandTotal,
                OneWayFee = vehicle.discountedOneDirectionPrice,
                DepositPrice = vehicle.blockedAmountForCreditCard,
                IsAvailable = true,
                CurrencyCode = "TRY", 
                TransmissionType = MapTransmission(vehicle.vehicleGroup?.gearType),
                VehicleCategoryType = MapCategory(vehicle.vehicleGroup?.vehicleGroupClass),
                PassangerQuantityType = MapPassanger(vehicle.vehicleGroup?.adultCapacity ?? 0),
                BaggageQuantityType = MapBaggage(vehicle.vehicleGroup?.suiteCaseCapacityTotal ?? 0),
                VehicleImages = new List<KolayCAR.Broker.Domain.Models.VehicleImage>
                {
                    new KolayCAR.Broker.Domain.Models.VehicleImage { Url = vehicle.vehicleGroup?.image }
                }
            };

            return result;
        }

        public static List<KolayCAR.Broker.Domain.Models.Vehicle> Map(this List<KolayCAR.Broker.Domain.Models.Responses.YesOto.YesOtoVehicleData> vehicles)
        {
            var result = new List<KolayCAR.Broker.Domain.Models.Vehicle>();

            if (vehicles != null)
            {
                foreach (var vehicle in vehicles)
                {
                    result.Add(vehicle.Map());
                }
            }

            return result;
        }

        private static KolayCAR.Broker.Domain.Models.TransmissionTypes MapTransmission(string gearType)
        {
            if (string.IsNullOrEmpty(gearType)) return KolayCAR.Broker.Domain.Models.TransmissionTypes.None;

            if (gearType.ToLower().Contains("otomatik")) return KolayCAR.Broker.Domain.Models.TransmissionTypes.Automatic;
            if (gearType.ToLower().Contains("manuel")) return KolayCAR.Broker.Domain.Models.TransmissionTypes.Manuel;

            return KolayCAR.Broker.Domain.Models.TransmissionTypes.None;
        }

        private static KolayCAR.Broker.Domain.Models.VehicleCategoryTypes MapCategory(string category)
        {
            if (string.IsNullOrEmpty(category)) return KolayCAR.Broker.Domain.Models.VehicleCategoryTypes.None;

            var cat = category.ToLower();
            if (cat.Contains("ekonomik")) return KolayCAR.Broker.Domain.Models.VehicleCategoryTypes.Economic;
            if (cat.Contains("kompakt")) return KolayCAR.Broker.Domain.Models.VehicleCategoryTypes.Compact;
            if (cat.Contains("standart")) return KolayCAR.Broker.Domain.Models.VehicleCategoryTypes.Standard;
            if (cat.Contains("lüks")) return KolayCAR.Broker.Domain.Models.VehicleCategoryTypes.Luxury;
            if (cat.Contains("orta")) return KolayCAR.Broker.Domain.Models.VehicleCategoryTypes.Intermediate;
            if (cat.Contains("üst")) return KolayCAR.Broker.Domain.Models.VehicleCategoryTypes.FullSize;

            return KolayCAR.Broker.Domain.Models.VehicleCategoryTypes.None;
        }

        private static KolayCAR.Broker.Domain.Models.PassangerQuantityTypes MapPassanger(int capacity)
        {
            return capacity switch
            {
                1 => KolayCAR.Broker.Domain.Models.PassangerQuantityTypes.OnePerson,
                2 => KolayCAR.Broker.Domain.Models.PassangerQuantityTypes.TwoPerson,
                3 => KolayCAR.Broker.Domain.Models.PassangerQuantityTypes.ThreePerson,
                4 => KolayCAR.Broker.Domain.Models.PassangerQuantityTypes.FourPerson,
                5 => KolayCAR.Broker.Domain.Models.PassangerQuantityTypes.FivePerson,
                6 => KolayCAR.Broker.Domain.Models.PassangerQuantityTypes.SixPerson,
                7 => KolayCAR.Broker.Domain.Models.PassangerQuantityTypes.SevenPerson,
                8 => KolayCAR.Broker.Domain.Models.PassangerQuantityTypes.EightPerson,
                9 => KolayCAR.Broker.Domain.Models.PassangerQuantityTypes.NinePerson,
                _ => KolayCAR.Broker.Domain.Models.PassangerQuantityTypes.None
            };
        }

        private static KolayCAR.Broker.Domain.Models.BaggageQuantityTypes MapBaggage(int capacity)
        {
            return capacity switch
            {
                1 => KolayCAR.Broker.Domain.Models.BaggageQuantityTypes.One,
                2 => KolayCAR.Broker.Domain.Models.BaggageQuantityTypes.Two,
                3 => KolayCAR.Broker.Domain.Models.BaggageQuantityTypes.Three,
                4 => KolayCAR.Broker.Domain.Models.BaggageQuantityTypes.Four,
                5 => KolayCAR.Broker.Domain.Models.BaggageQuantityTypes.Five,
                _ => KolayCAR.Broker.Domain.Models.BaggageQuantityTypes.None
            };
        }
    }
}
