using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Ekar2
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this Ekar2ResponseBase.Vehicle vehicle)
        {
            return new Vehicle
            {
                VehicleId = vehicle.id,
                VehicleName = vehicle.name,
                VehicleBrandName = vehicle.vehicleBrand,
                VehicleModelName = vehicle.vehicleModel,
                VehicleCode = vehicle.id.ToString(),
                FuelType = GetFuelType(vehicle.vehicleFuelType),
                FuelTypeName = vehicle.vehicleFuelType,
                SippCode = vehicle.sipp,
                TransmissionType = GetTransmissionType(vehicle.vehicleGearbox),
                TransmissionTypeName = vehicle.vehicleGearbox,
                VendorMinimumDriverAge = vehicle.minDriverAge,
                VendorMinimumDrivingLicenseAge = vehicle.minLicenseYear,
                PassangerQuantityType = GetPassangerQuantityType(vehicle.passenger),
                VehicleCategoryType = GetCategoryType(vehicle.vehicleClass),
                VehicleCategoryTypeName = vehicle.vehicleClass,
                VehicleDescription = vehicle.name + " - " + vehicle.sipp + " - " + vehicle.vehicleClass,
                DepositPrice = (float)vehicle.blockedAmount,
                IsAvailable = true
            };
        }
        public static Vehicle Map(this Ekar2ResponseBase.AvailableVehicle vehicle, ResponseReservationStepsAdditionalInformation additionalInformation, Domain.Models.Vendor vendor)
        {
            return new Vehicle
            {
                VehicleId = vehicle.vehicleGroupId,
                VehicleName = vehicle.vehicleGroupName,
                VehicleCode = vehicle.vehicleGroupId.ToString(),
                FuelType = GetFuelType(vehicle.vehicleFuelType),
                FuelTypeName = vehicle.vehicleFuelType,
                SippCode = vehicle.sipp,
                TransmissionType = GetTransmissionType(vehicle.vehicleGearbox),
                TransmissionTypeName = vehicle.vehicleGearbox,
                VendorMinimumDriverAge = vehicle.minDriverAge,
                VendorMinimumDrivingLicenseAge = vehicle.minLicenseYear,
                PassangerQuantityType = GetPassangerQuantityType(vehicle.passenger),
                VehicleCategoryType = GetCategoryType(vehicle.vehicleClass),
                VehicleCategoryTypeName = vehicle.vehicleClass,
                DepositPrice = (float)vehicle.blockedAmount,
                TotalKMLimit = vehicle.kilometerLimit,
                RentalDuration = vehicle.rentalDay,
                OneWayFee = (float)vehicle.oneWayPrice,
                DailyPrice = (float)vehicle.dailyRentalPrice,
                DailyPricePayNow = (float)vehicle.dailyRentalPrice,
                TotalPrice = (float)vehicle.totalPrice,
                CurrencyCode = vehicle.currencyCode,
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VendorLogo = additionalInformation.Vendor.Logo,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice,
                IsAvailable = true
            };
        }


        public static List<Vehicle> Map(this List<Ekar2ResponseBase.Vehicle> vehicles)
        {
            if (vehicles != null && vehicles.Count > 0)
            {
                var vehicleList = new List<Vehicle>();
                foreach (var vehicle in vehicles)
                {
                    vehicleList.Add(vehicle.Map());
                }
                return vehicleList;
            }
            return null;
        }
        public static List<Vehicle> Map(this List<Ekar2ResponseBase.AvailableVehicle> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, Domain.Models.Vendor vendor)
        {
            if (vehicles != null && vehicles.Count > 0)
            {
                var vehicleList = new List<Vehicle>();
                foreach (var vehicle in vehicles)
                {
                    vehicleList.Add(vehicle.Map(additionalInformation, vendor));
                }
                return vehicleList;
            }
            return null;
        }
        private static TransmissionTypes GetTransmissionType(string transmissionType)
        {
            switch (transmissionType)
            {
                case "OTOMATİK":
                    return TransmissionTypes.Automatic;
                case "MANUEL":
                    return TransmissionTypes.Manuel;
                default:
                    return TransmissionTypes.None;
            }
        }
        private static VehicleCategoryTypes GetCategoryType(string categoryType)
        {
            switch (categoryType)
            {
                case "EKONOMİ":
                    return VehicleCategoryTypes.Economic;
                case "SUV":
                    return VehicleCategoryTypes.SUV;
                case "LÜKS":
                    return VehicleCategoryTypes.Luxury;
                case "KONFOR":
                    return VehicleCategoryTypes.Compact;
                case "PREMIUM":
                    return VehicleCategoryTypes.Premium;
                case "ORTA":
                    return VehicleCategoryTypes.Standard;
                default:
                    return VehicleCategoryTypes.None;
            }
        }
        private static FuelTypes GetFuelType(string fuelType)
        {
            switch (fuelType)
            {
                case "BENZİN":
                    return FuelTypes.Gasoline;
                case "DİZEL":
                    return FuelTypes.Diesel;
                case "BENZİN HYBRID":
                    return FuelTypes.HybritGasoline;
                default:
                    return FuelTypes.None;
            }
        }

        private static PassangerQuantityTypes GetPassangerQuantityType(int passenger_capacity)
        {
            return passenger_capacity switch
            {
                2 => PassangerQuantityTypes.TwoPerson,
                3 => PassangerQuantityTypes.ThreePerson,
                4 => PassangerQuantityTypes.FourPerson,
                5 => PassangerQuantityTypes.FivePerson,
                6 => PassangerQuantityTypes.SixPerson,
                7 => PassangerQuantityTypes.SevenPerson,
                8 => PassangerQuantityTypes.EightPerson,
                9 => PassangerQuantityTypes.NinePerson,
                10 => PassangerQuantityTypes.TenPerson,
                11 => PassangerQuantityTypes.ElevenPerson,
                12 => PassangerQuantityTypes.TwelvePerson,
                13 => PassangerQuantityTypes.ThirteenPerson,
                14 => PassangerQuantityTypes.FourteenPerson,
                15 => PassangerQuantityTypes.FifteenPerson,
                16 => PassangerQuantityTypes.SixteenPerson,
                17 => PassangerQuantityTypes.SeventeenPerson,
                18 => PassangerQuantityTypes.EighteenPerson,
                19 => PassangerQuantityTypes.NineteenPerson,
                20 => PassangerQuantityTypes.TwentyPerson,
                _ => PassangerQuantityTypes.OnePerson,
            };
        }
    }
}
