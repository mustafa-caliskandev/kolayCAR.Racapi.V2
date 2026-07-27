using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Responses.RentGo;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Kolaycar.Broker.Api.Mappers.RentGo
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this RentGoVehicleListingItem vehicle, string listId, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, float onewayFee) =>
            vehicle != null ? new Vehicle
            {
                VehicleId = 0,
                VehicleCode = vehicle.Version.Id,
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = vehicle.Title,
                VendorLogo = additionalInformation.Vendor.Logo,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                DailyPrice = (float)((vehicle.PayNow) / (additionalInformation.RentalDuration > 0 ? additionalInformation.RentalDuration : 1)),
                OneWayFee = onewayFee,
                TotalPrice = (float)(vehicle.PayNow + vehicle.PayOffice),
                DailyPricePayNow = (float)(vehicle.PayNow / (additionalInformation.RentalDuration > 0 ? additionalInformation.RentalDuration : 1)),
                TotalPricePayNow = (float)vehicle.PayNow,
                DepositPrice = (float)(vehicle.VehicleGroup?.Deposit ?? 0),
                IsAvailable = true,
                RentalDuration = additionalInformation.RentalDuration,
                TransmissionType = GetTransmissionType(vehicle.Version?.Transmission),
                TransmissionTypeName = vehicle.Version?.Transmission,
                FuelType = GetFuelType(vehicle.Version?.Fuel),
                FuelTypeName = vehicle.Version?.Fuel,
                PassangerQuantityName = vehicle.Version?.Seat,
                PassangerQuantityType = GetPassangerQuantityType(vehicle.Version?.Seat?.ToIntNullSafe() ?? 0),
                BaggageQuantityName = vehicle.Version?.Luggage.ToString(),
                BaggageQuantityType = GetBaggageQuantityType(vehicle.Version?.Luggage ?? 0),
                VendorMinimumDriverAge = vehicle.VehicleGroup?.MinAge ?? 0,
                VendorMinimumDrivingLicenseAge = vehicle.VehicleGroup?.MinExp ?? 0,
                FullCredit = vendor.CreditType == CreditType.FullCredit,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice,
                VehicleImages = new List<VehicleImage> { new VehicleImage { Url = vehicle.Photo } },
                DailyKMLimit = vehicle?.VehicleGroup?.MaxDailyKm ?? 0
            }
            : null;

        public static List<Vehicle> Map(this List<RentGoVehicleListingItem> vehicles, string listId, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, float onewayFee) => vehicles?.Select(vehicle => vehicle.Map(listId, additionalInformation, vendor, onewayFee)).ToList() ?? new List<Vehicle>();

        public static List<Vehicle> Map(this List<RentGoVersion> vehicles, Vendor vendor) => vehicles?.Select(vehicle => vehicle.Map(vendor)).ToList() ?? new List<Vehicle>();


        public static Vehicle Map(this RentGoVersion vehicle, Vendor vendor) =>
               vehicle != null ? new Vehicle
               {
                   VendorName = vendor.VendorName,
                   VehicleCode = vehicle.VersionId,
                   VehicleName = $"{vehicle.BrandName} {vehicle.ModelName} ({vehicle.FuelType}-{vehicle.TransmissionType})",
                   SippCode = vehicle.Sipp,
                   FuelTypeName = vehicle.FuelType,
                   FuelType = GetFuelType(vehicle.FuelType),
                   TransmissionTypeName = vehicle.TransmissionType,
                   TransmissionType = GetTransmissionType(vehicle.TransmissionType)
               }
               : null;

        public static FuelTypes GetFuelType(string fuelTypeName)
        {
            if (string.IsNullOrEmpty(fuelTypeName)) return FuelTypes.Gasoline;
            switch (fuelTypeName.ToLowerInvariant())
            {
                case "dizel":
                case "diesel":
                    return FuelTypes.Diesel;
                case "elektrik":
                case "electric":
                    return FuelTypes.Electric;
                case "hibrit":
                case "hybrid":
                    return FuelTypes.Hybrid;
                default:
                case "benzin":
                case "petrol":
                case "gasoline":
                    return FuelTypes.Gasoline;
            }
        }

        public static TransmissionTypes GetTransmissionType(string transmissionTypeName)
        {
            if (string.IsNullOrEmpty(transmissionTypeName)) return TransmissionTypes.Manuel;
            switch (transmissionTypeName.ToLowerInvariant())
            {
                case "otomatik":
                case "automatic":
                case "auto":
                case "semi_automatic":
                case "yarı_otomatik":
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
