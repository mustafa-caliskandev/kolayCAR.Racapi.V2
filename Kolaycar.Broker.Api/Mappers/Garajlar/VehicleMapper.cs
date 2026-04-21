using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using static KolayCAR.Broker.Domain.Models.Response.GarajlarResponseBase;

namespace KolayCAR.Broker.API.Mappers.Garajlar
{
    public static class VehicleMapper
    {
        public static List<Vehicle> Map(this IEnumerable<GarajlarVehicle> vehicles, Vendor vendor)
        {
            if (vendor == null)
                return new List<Vehicle>();

            return vehicles?.Select(vehicle => new Vehicle
            {
                VendorId = vendor.VendorId,
                VendorName = vendor.VendorName,
                VehicleCode = vehicle.sub_group_short_name,
                VehicleName = vehicle.sub_group_name,
                FuelTypeName = vehicle.fuel_type,
                TransmissionTypeName = vehicle.transmission_type,
                DepositPrice = vehicle.provision_amount.ToFloatNullSafe(),
                VendorMinimumDriverAge = vehicle.min_driver_age.ToIntNullSafe(),
                VendorMinimumDrivingLicenseAge = vehicle.min_driving_license_year.ToIntNullSafe()
            }).ToList() ?? new List<Vehicle>();
        }


        public static List<Vehicle> Map(this List<AvailabilityVehiclesResponse> vehicles, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation)
        {
            if (vendor == null)
                return new List<Vehicle>();

            return (List<Vehicle>)(vehicles?.Select(vehicle => new Vehicle
            {
                VehicleCode = vehicle.sub_group_short_name,
                VendorId = vendor.VendorId,
                VendorName = vendor.VendorName,
                VendorPhone = vendor.VendorPhone,
                VendorEmail = vendor.VendorEmail,
                VehicleName = vehicle.sub_group_name,
                VendorLogo = vendor.Logo,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = vehicle.days,
                DailyPrice = vehicle.daily_price.ToFloatNullSafe(),
                OneWayFee = vehicle.drop_price.ToFloatNullSafe(),
                TotalPrice = vehicle.grand_total.ToFloatNullSafe(),
                IsAvailable = true,
                VehicleType = VehicleTypes.None,
                TransmissionType = GetTransmissionType(vehicle.transmission_type),
                VehicleCategoryType = VehicleCategoryTypes.None,
                PassangerQuantityType = PassangerQuantityTypes.None,
                FuelType = GetFuelType(vehicle.fuel_type),
                BaggageQuantityType = BaggageQuantityTypes.None,
                VendorMinimumDriverAge = vehicle.min_driver_age,
                VendorMinimumDrivingLicenseAge = vehicle.min_driving_license_year,
                DailyPricePayNow = vehicle.daily_price.ToFloatNullSafe(),
                TotalPricePayNow = vehicle.total_discount.ToFloatNullSafe(),
                DepositPrice = vehicle.provision_amount.ToFloatNullSafe(),
                DailyKMLimit = (vehicle.total_km_limit / vehicle.days).ToFloatNullSafe().ToIntNullSafe(),
                TotalKMLimit = vehicle.total_km_limit,
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                Extras = GetExtras(vehicle.extras),
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
            }).ToList() ?? new List<Vehicle>());
        }

        private static List<Extra> GetExtras(List<GarajlarExtra> extras) =>
            extras?.Select(extra => new Extra
             {
                 ExtraCode = extra.code,
                 ExtraName = extra.name,
                 ExtraDescription = extra.description,
                 Price = extra.price,
                 ExtraRentalType = ExtraRentalTypes.Daily
             }).ToList() ?? new List<Extra>();


        private static FuelTypes GetFuelType(string fuel)
        {
            return fuel switch
            {
                "Dizel" => FuelTypes.Diesel,
                "Benzin" => FuelTypes.Gasoline,
                _ => FuelTypes.None
            };
        }

        public static TransmissionTypes GetTransmissionType(string transmission)
        {
            return transmission switch
            {
                "Otomatik" => TransmissionTypes.Automatic,
                "Manuel" => TransmissionTypes.Manuel,
                _ => TransmissionTypes.None
            };
        }
    }
}
