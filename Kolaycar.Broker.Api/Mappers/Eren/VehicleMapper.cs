using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Responses.Eren;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Kolaycar.Broker.Api.Mappers.Eren
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this ErenVehicleGroup vehicle, string vendorName) =>
            vehicle != null ? new Vehicle
            {
                VendorName = vendorName,
                VehicleId = 0,
                VehicleCode = vehicle.GroupId,
                VehicleName = $"{vehicle.CarMake} - {vehicle.CarModel} - {vehicle.FuelType} - {vehicle.TransmissionType}",
                FuelType = GetFuelType(vehicle.FuelType),
                FuelTypeName = vehicle.FuelType,
                TransmissionType = GetTransmissionType(vehicle.TransmissionType),
                TransmissionTypeName = vehicle.TransmissionType,
                SippCode = vehicle.SippCode,
                DepositPrice = vehicle.Deposit.ToFloatNullSafe(),
                PassangerQuantityName = vehicle.Seats,
                PassangerQuantityType = GetPassangerQuantityType(vehicle.Seats.ToIntNullSafe()),
                BaggageQuantityName = vehicle.BigBags,
                BaggageQuantityType = GetBaggageQuantityType(vehicle.BigBags.ToIntNullSafe()),
                VehicleImages = new List<VehicleImage> { new VehicleImage { Url = vehicle.CarImage } }
            }
            : null;

        public static List<Vehicle> Map(this List<ErenVehicleGroup> vehicles, string vendorName)
        {
            return vehicles?.Select(vehicle => vehicle.Map(vendorName)).ToList() ?? new List<Vehicle>();
        }

        public static Vehicle Map(this ErenAvailableCar vehicle, string searchRequestId, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
            vehicle != null ? new Vehicle
            {
                VehicleId = vehicle.GroupId,
                VehicleCode = vehicle.GroupId.ToString(),
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = $"{vehicle.CarMake} {vehicle.CarModel}",
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = vehicle.SippCode,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = vehicle.TotalDays,
                DailyPrice = vehicle.DailyPrice.ToFloatNullSafe(),
                OneWayFee = vehicle.DropPrice.ToFloatNullSafe(),
                TotalPrice = vehicle.TotalPrice.ToFloatNullSafe(),
                TotalKMLimit = vehicle.MileageMax,
                IsAvailable = vehicle.TotalPrice.ToFloatNullSafe() > 0,
                VehicleType = VehicleTypes.None,
                TransmissionType = GetTransmissionType(vehicle.TransmissionType),
                TransmissionTypeName = vehicle.TransmissionType,
                VehicleCategoryType = VehicleCategoryTypes.None,
                PassangerQuantityType = GetPassangerQuantityType(vehicle.Seats),
                PassangerQuantityName = vehicle.Seats.ToString(),
                FuelType = GetFuelType(vehicle.FuelType),
                FuelTypeName = vehicle.FuelType,
                IsThereAirCondition = true,
                VendorMinimumDriverAge = vehicle.MinDriverAge,
                VendorMinimumDrivingLicenseAge = vehicle.MinDrivingLicenseAge,
                DailyPricePayNow = vehicle.DailyPrice.ToFloatNullSafe(),
                TotalPricePayNow = vehicle.TotalPrice.ToFloatNullSafe(),
                DepositPrice = vehicle.Deposit.ToFloatNullSafe(),
                FullCredit = vendor.CreditType == CreditType.FullCredit,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice,
                VehicleImages = new List<VehicleImage> { new VehicleImage { Url = vehicle.CarImage } },
                BaggageQuantityName = vehicle.BigBags.ToString(),
                BaggageQuantityType = GetBaggageQuantityType(vehicle.BigBags),
                Extras = vehicle.Extras?.MapErenExtras()
            }
            : null;

        public static List<Vehicle> Map(this List<ErenAvailableCar> vehicles, string searchRequestId, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            return vehicles?.Select(vehicle => vehicle.Map(searchRequestId, additionalInformation, vendor)).ToList() ?? new List<Vehicle>();
        }

        public static List<Extra> MapErenExtras(this List<ErenExtra> extras)
        {
            return extras?.Select(extra => new Extra
            {
                ExtraId = extra.ServiceId,
                ExtraCode = extra.ServiceId.ToString(),
                ExtraName = extra.ServiceName,
                Price = extra.Price.ToFloatNullSafe(),
                ExtraRentalType = extra.PerDay == 1 ? ExtraRentalTypes.Daily : ExtraRentalTypes.PerRental
            }).ToList() ?? new List<Extra>();
        }

        public static FuelTypes GetFuelType(string fuelTypeName)
        {
            if (string.IsNullOrEmpty(fuelTypeName)) return FuelTypes.Gasoline;

            switch (fuelTypeName.ToLowerInvariant())
            {
                case "diesel":
                case "dizel":
                    return FuelTypes.Diesel;
                case "hybrid":
                case "hibrit":
                    return FuelTypes.Hybrid;
                case "electric":
                case "elektrik":
                    return FuelTypes.Electric;
                default:
                case "petrol":
                case "benzin":
                    return FuelTypes.Gasoline;
            }
        }

        public static TransmissionTypes GetTransmissionType(string transmissionTypeName)
        {
            if (string.IsNullOrEmpty(transmissionTypeName)) return TransmissionTypes.Manuel;

            switch (transmissionTypeName.ToLowerInvariant())
            {
                case "automatic":
                case "otomatik":
                    return TransmissionTypes.Automatic;
                default:
                case "manuel":
                case "manual":
                    return TransmissionTypes.Manuel;
            }
        }

        public static PassangerQuantityTypes GetPassangerQuantityType(int seats)
        {
            return seats switch
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
                _ => PassangerQuantityTypes.OnePerson,
            };
        }

        public static BaggageQuantityTypes GetBaggageQuantityType(int bags)
        {
            return bags switch
            {
                2 => BaggageQuantityTypes.Two,
                3 => BaggageQuantityTypes.Three,
                4 => BaggageQuantityTypes.Four,
                5 => BaggageQuantityTypes.Five,
                6 => BaggageQuantityTypes.Six,
                7 => BaggageQuantityTypes.Seven,
                8 => BaggageQuantityTypes.Eight,
                _ => BaggageQuantityTypes.One,
            };
        }
    }
}
