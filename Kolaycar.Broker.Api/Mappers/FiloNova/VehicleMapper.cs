using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.FiloNovaResponseBase;
namespace KolayCAR.Broker.API.Mappers.FiloNova
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this AvailabilityData vehicle,
           FiloNovaResponseBase filoNovaResponseBase,
           ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var apiRentalDuration = filoNovaResponseBase.totalDuration.ToFloatNullSafe();
            var rentalDuration = (int)Math.Ceiling(apiRentalDuration);

            //int rentalDuration = CalculationHelper.RoundPrice(filoNovaResponseBase.totalDuration.ToFloatNullSafe(), (int)PriceRoundingTypes.RoundUp).ToIntNullSafe(); // gkursad
            /*var dailyPrice = vehicle.dailyAmount > 0 
                ? vehicle.dailyAmount.ToFloatNullSafe()
                : vehicle.payAmount.ToFloatNullSafe() / rentalDuration; // gkursad*/

            var dailyPrice = vehicle.payAmount.ToFloatNullSafe() / rentalDuration; // gkursad (Yarım & Çeyrek gün talebi)

            return vehicle != null ? new Vehicle
            {
                VehicleId = 0,
                VehicleCode = vehicle.groupCodeId.ToString(),
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = vehicle.groupCodeName,
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = string.Empty,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = rentalDuration,
                DailyPrice = dailyPrice,
                OneWayFee = filoNovaResponseBase.oneWayFeeAmount.ToFloatNullSafe(),
                TotalPrice = vehicle.payAmount.ToFloatNullSafe(),
                IsAvailable = vehicle.payAmount > 0,
                VehicleImages = new List<VehicleImage> { },
                VehicleType = VehicleTypes.None,
                TransmissionType = TransmissionTypes.None,
                VehicleCategoryType = VehicleCategoryTypes.None,
                PassangerQuantityType = PassangerQuantityTypes.None,
                FuelType = FuelTypes.None,
                BaggageQuantityType = BaggageQuantityTypes.None,
                IsThereAirCondition = true,
                VendorMinimumDriverAge = 0,
                VendorMinimumDrivingLicenseAge = 0,
                DailyPricePayNow = dailyPrice,
                TotalKMLimit = vehicle.kmLimit.ToIntNullAvailable(),
                TotalPricePayNow = vehicle.payAmount.ToFloatNullSafe(),
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                Extras = null,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
            }
            : null;
        }

        public static List<Vehicle> Map(this List<AvailabilityData> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, FiloNovaResponseBase filoNovaResponseBase, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(filoNovaResponseBase: filoNovaResponseBase, additionalInformation: additionalInformation, vendor));

            return _vehicles;
        }

        public static Vehicle Map(this GroupCodeInformation vehicle) =>
         vehicle != null ? new Vehicle
         {
             VendorName = "FiloNova",
             VehicleCode = vehicle.groupCodeId,
             VehicleName = $"{vehicle.showRoomBrandName} {vehicle.showRoomModelName} ({vehicle.fuelTypeName}-{vehicle.transmissionName})",
             SippCode = vehicle.groupCodeName,
             FuelTypeName = vehicle.fuelTypeName,
             TransmissionTypeName = vehicle.transmissionName,
             DepositPrice = vehicle.depositAmount.ToFloatNullSafe(),
             VendorMinimumDriverAge = vehicle.minimumAge,
             VendorMinimumDrivingLicenseAge = vehicle.minimumDriverLicense
         }
         : null;

        public static List<Vehicle> Map(this List<GroupCodeInformation> vehicles)
        {
            var _vehicles = new List<Vehicle>();
            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map());

            return _vehicles;
        }

    }
}
