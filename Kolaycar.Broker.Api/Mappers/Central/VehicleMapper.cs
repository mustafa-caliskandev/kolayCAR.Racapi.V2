using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.CentralResponseBase;

namespace KolayCAR.Broker.API.Mappers.Central
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this CentralVehicle vehicle,
            ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
            vehicle != null ? new Vehicle
            {
                VehicleId = vehicle.SubGroupId.ToIntNullSafe(),
                VehicleCode = vehicle.SubGroupShortName,
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = $"{vehicle.MakeName} {vehicle.ModelName}",
                VendorLogo = additionalInformation.Vendor.Logo,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = vehicle.Days.ToIntNullSafe(),
                DailyPrice = vehicle.DiscountedDailyPrice.ToFloatNullSafe(),
                OneWayFee = vehicle.DropPrice.ToFloatNullSafe(),
                TotalPrice = vehicle.DiscountedDailyPrice.ToFloatNullSafe() * vehicle.Days.ToIntNullSafe(),
                IsAvailable = true,
                VehicleImages = new List<VehicleImage>
                {
                    new VehicleImage
                    {
                        Url = vehicle.VehiclePhoto
                    }
                },
                VehicleType = VehicleTypes.None,
                TransmissionType = GetCentralTransmissionType(vehicle.Transmission),
                VehicleCategoryType = GetCentralVehicleCategoryType(vehicle.CarClass),
                VehicleCategoryTypeName = vehicle.CarClass,
                PassangerQuantityName = vehicle.PassengerCapacity,
                PassangerQuantityType = GetCentralPassangerQuantityType(vehicle.PassengerCapacity.ToIntNullSafe()),
                FuelType = GetCentralFuelType(vehicle.FuelType),
                BaggageQuantityName = vehicle.BaggageCapacity,
                BaggageQuantityType = GetcentralBaggageQuantityType(vehicle.BaggageCapacity.ToIntNullSafe()),
                IsThereAirCondition = true,
                VendorMinimumDriverAge = vehicle.DriverMinAge.ToIntNullSafe(),
                VendorMinimumDrivingLicenseAge = vehicle.DriverMinLicenceYear.ToIntNullSafe(),
                DailyPricePayNow = vehicle.DiscountedDailyPrice.ToFloatNullSafe(),
                TotalPricePayNow = vehicle.AdditionalGrandTotal.ToFloatNullSafe(),
                DepositPrice = vehicle.ProvisionAmountTRY.ToFloatNullSafe(),
                TotalKMLimit = vehicle.TotalKmLimit.ToIntNullAvailable(),
                Extras = null,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
            }
            : null;

        public static List<Vehicle> Map(this List<CentralVehicle> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(additionalInformation: additionalInformation, vendor));

            return _vehicles;
        }

        public static Vehicle Map(this CentralVehicle vehicle) =>
            vehicle != null ? new Vehicle
            {
                VendorName = "Central",
                VehicleId = vehicle.SubGroupId.ToIntNullSafe(),
                VehicleCode = vehicle.SubGroupShortName,
                VehicleName = $"{vehicle.SubGroupName} - {vehicle.FuelType} - {vehicle.Transmission}",
                FuelTypeName = vehicle.FuelType,
                TransmissionTypeName = vehicle.Transmission,
                VendorMinimumDriverAge = vehicle.DriverMinAge.ToIntNullSafe(),
                VendorMinimumDrivingLicenseAge = vehicle.DriverMinLicenceYear.ToIntNullSafe(),
                DepositPrice = vehicle.ProvisionAmountTRY.ToFloatNullAvailable()
            }
            : null;

        public static List<Vehicle> Map(this List<CentralVehicle> vehicles)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map());

            return _vehicles;
        }

        public static VehicleCategoryTypes GetCentralVehicleCategoryType(string categoryTypeName)
        {
            return categoryTypeName switch
            {
                "ECONOMIC" => VehicleCategoryTypes.Economic,
                "COMPACT" => VehicleCategoryTypes.Compact,
                "SUV" => VehicleCategoryTypes.SUV,
                "INTERMEDIATE" => VehicleCategoryTypes.Intermediate,
                "FULLSIZE" => VehicleCategoryTypes.FullSize,
                "LUXURY" => VehicleCategoryTypes.Luxury,
                "VAN" => VehicleCategoryTypes.Minivan,
                _ => VehicleCategoryTypes.None,
            };
        }

        public static FuelTypes GetCentralFuelType(string fuelTypeName)
        {
            switch (fuelTypeName)
            {
                default:
                case "Dizel":
                    return FuelTypes.Diesel;
                case "Benzin":
                    return FuelTypes.Gasoline;
            }
        }

        public static TransmissionTypes GetCentralTransmissionType(string transmissionTypeName)
        {
            switch (transmissionTypeName)
            {
                default:
                case "Otomatik":
                    return TransmissionTypes.Automatic;
                case "Manuel":
                    return TransmissionTypes.Manuel;
            }
        }

        public static PassangerQuantityTypes GetCentralPassangerQuantityType(int passangerQuantityName)
        {
            switch (passangerQuantityName)
            {
                default:
                case 1:
                    return PassangerQuantityTypes.OnePerson;
                case 2:
                    return PassangerQuantityTypes.TwoPerson;
                case 3:
                    return PassangerQuantityTypes.ThreePerson;
                case 4:
                    return PassangerQuantityTypes.FourPerson;
                case 5:
                    return PassangerQuantityTypes.FivePerson;
                case 6:
                    return PassangerQuantityTypes.SixPerson;
                case 7:
                    return PassangerQuantityTypes.SevenPerson;
                case 8:
                    return PassangerQuantityTypes.EightPerson;
                case 9:
                    return PassangerQuantityTypes.NinePerson;
                case 10:
                    return PassangerQuantityTypes.TenPerson;
                case 11:
                    return PassangerQuantityTypes.ElevenPerson;
                case 12:
                    return PassangerQuantityTypes.TwelvePerson;
                case 13:
                    return PassangerQuantityTypes.ThirteenPerson;
                case 14:
                    return PassangerQuantityTypes.FourteenPerson;
                case 15:
                    return PassangerQuantityTypes.FifteenPerson;
                case 16:
                    return PassangerQuantityTypes.SixteenPerson;
                case 17:
                    return PassangerQuantityTypes.SeventeenPerson;
                case 18:
                    return PassangerQuantityTypes.EighteenPerson;
                case 19:
                    return PassangerQuantityTypes.NineteenPerson;
                case 20:
                    return PassangerQuantityTypes.TwentyPerson;
            }
        }

        public static BaggageQuantityTypes GetcentralBaggageQuantityType(int baggageQuantityName)
        {
            switch (baggageQuantityName)
            {
                default:
                case 1:
                    return BaggageQuantityTypes.One;
                case 2:
                    return BaggageQuantityTypes.Two;
                case 3:
                    return BaggageQuantityTypes.Three;
                case 4:
                    return BaggageQuantityTypes.Four;
                case 5:
                    return BaggageQuantityTypes.Five;
                case 6:
                    return BaggageQuantityTypes.Six;
                case 7:
                    return BaggageQuantityTypes.Seven;
                case 8:
                    return BaggageQuantityTypes.Eight;
            }
        }
    }
}
