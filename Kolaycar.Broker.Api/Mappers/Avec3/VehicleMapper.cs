using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Avec3
{
    public static class VehicleMapper
    {
        public static List<Vehicle> Map(this List<Avec3ResponseBase.Vehicle> vehicles)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map());

            return _vehicles;
        }
        public static Vehicle Map(this Avec3ResponseBase.Vehicle vehicle) =>
          vehicle != null ? new Vehicle
          {
              VehicleId = vehicle.id,
              VehicleName = vehicle.name + " - " + vehicle.fuel_type + " - " + vehicle.gear_type,
              VehicleBrandName = vehicle.brand.name,
              VehicleCode = vehicle.id.ToString(),
              FuelType = GetAvecFuelType(vehicle.fuel_type),
              FuelTypeName = vehicle.fuel_type,
              SippCode = vehicle.segment_code,
              TransmissionType = GetAvecTransmissionType(vehicle.gear_type),
              TransmissionTypeName = vehicle.gear_type,
              VendorMinimumDriverAge = vehicle.min_driver_age,
              VendorMinimumDrivingLicenseAge = vehicle.min_driver_license_age,
              PassangerQuantityType = GetAvecPassangerQuantityType(vehicle.passenger_capacity),
              VehicleCategoryType = GetAvecCategoryType(vehicle.vehicle_group),
              VehicleCategoryTypeName = vehicle.vehicle_group,
              VehicleDescription = vehicle.name + vehicle.segment_text,
              DepositPrice = (float)vehicle.deposit_price,
              DailyKMLimit = (int)vehicle.daily_km_limit

          } : null;
        public static List<Vehicle> Map(this List<Avec3ResponseBase.AvailableVehicle> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(additionalInformation: additionalInformation, vendor));

            return _vehicles;
        }
        public static Vehicle Map(this Avec3ResponseBase.AvailableVehicle vehicle,
        ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
        vehicle != null ? new Vehicle
        {
            VehicleId = vehicle.vehicle.id,
            VehicleName = vehicle.vehicle.name,
            VehicleBrandName = vehicle.vehicle.brand.name,
            VehicleCode = vehicle.vehicle.id.ToStringNullSafe(),
            FuelType = GetAvecFuelType(vehicle.vehicle.fuel_type),
            FuelTypeName = vehicle.vehicle.fuel_type,
            SippCode = vehicle.vehicle.segment_code,
            TransmissionType = GetAvecTransmissionType(vehicle.vehicle.gear_type),
            TransmissionTypeName = vehicle.vehicle.gear_type,
            VendorMinimumDriverAge = vehicle.vehicle.min_driver_age,
            VendorMinimumDrivingLicenseAge = vehicle.vehicle.min_driver_license_age,
            PassangerQuantityType = GetAvecPassangerQuantityType(vehicle.vehicle.passenger_capacity),
            PassangerQuantityName = vehicle.vehicle.passenger_capacity.ToString(),
            VehicleCategoryType = GetAvecCategoryType(vehicle.vehicle.vehicle_group),
            VehicleCategoryTypeName = vehicle.vehicle.vehicle_group,
            VehicleDescription = vehicle.vehicle.name + vehicle.vehicle.segment_text,
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
            RentalDuration = vehicle.days,
            DailyPrice = vehicle.price_details.total_price / vehicle.days,
            OneWayFee = vehicle.price_details.dropoff_price,
            TotalPrice = vehicle.price_details.total_price,
            IsAvailable = true,
            VehicleType = VehicleTypes.None, //tüm araçlar sedan dönmektedir
            //BaggageQuantityType = vehicle.vehicle.large_suitcase_capacity, //sürekli 0 döndüğü için yoruma alındı
            //BaggageQuantityName = vehicle.vehicle.large_suitcase_capacity, //sürekli 0 döndüğü için yoruma alındı
            IsThereAirCondition = true,
            DailyPricePayNow = vehicle.price_details.total_price / vehicle.days,
            TotalPricePayNow = vehicle.price_details.total_price,
            DepositPrice = (float)vehicle.vehicle.deposit_price,
            DailyKMLimit = vehicle.vehicle.daily_km_limit.ToIntNullSafe(),
            TotalKMLimit = vehicle.total_km_limit.ToIntNullSafe(),
            FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
            RentalWorkingTypes = vendor.RentalWorkingType,
            ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
        }
        : null;




        private static VehicleCategoryTypes GetAvecCategoryType(string vehicle_group)
        {
            switch (vehicle_group)
            {
                default:
                case "Premium":
                    return VehicleCategoryTypes.Premium;
                case "Elite":
                    return VehicleCategoryTypes.CompactElite;
                case "Eco":
                    return VehicleCategoryTypes.Economic;
                case "Mid":
                    return VehicleCategoryTypes.Standard;
                case "SUV":
                    return VehicleCategoryTypes.SUV;

            }
        }

        public static FuelTypes GetAvecFuelType(string fuelTypeName)
        {
            switch (fuelTypeName)
            {
                default:
                case "diesel":
                    return FuelTypes.Diesel;
                case "petrol":
                    return FuelTypes.Gasoline;
            }
        }

        public static TransmissionTypes GetAvecTransmissionType(string transmissionTypeName)
        {
            switch (transmissionTypeName)
            {
                default:
                case "automatic":
                    return TransmissionTypes.Automatic;
                case "manuel":
                    return TransmissionTypes.Manuel;
            }
        }

        public static PassangerQuantityTypes GetAvecPassangerQuantityType(int passangerQuantityName)
        {
            return passangerQuantityName switch
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
