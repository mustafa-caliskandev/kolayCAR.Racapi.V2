using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Mappers.Garenta
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this GarentaResponseBase.VEHICLE vehicle,
            ResponseReservationStepsAdditionalInformation additionalInformation,
            List<GarentaResponseBase.ADD_PROD> addProd, Vendor vendor)
        {
            int rentalDuration = Math.Ceiling((additionalInformation.ReturnDateTime.Date - additionalInformation.PickupDateTime.Date).TotalDays).ToIntNullSafe();
            var oneWayFeeProduct = addProd != null && addProd.Any() ? addProd.Where(x => x.PRODUCT_ID == "HZM0020").FirstOrDefault() : null;
            float oneWayFee = oneWayFeeProduct != null ? oneWayFeeProduct.PRICE.ToFloatNullSafe() : 0;

            if ((additionalInformation.ReturnDateTime.TimeOfDay - additionalInformation.PickupDateTime.TimeOfDay).Hours > 0)
                rentalDuration++;

            var dailyPrice = (vehicle.NET_AMOUNT / rentalDuration).ToFloatNullSafe();

            return vehicle != null ? new Vehicle
            {
                VehicleId = 0,
                VehicleCode = vehicle.SIPP_CODE.ToString(),
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = vehicle.SIPP_CODE,
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = vehicle.SIPP_CODE,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = rentalDuration,
                DailyPrice = dailyPrice,
                OneWayFee = oneWayFee,
                TotalPrice = vehicle.NET_AMOUNT.ToFloatNullSafe(),
                IsAvailable = vehicle.NET_AMOUNT > 0,
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
                TotalPricePayNow = vehicle.NET_AMOUNT.ToFloatNullSafe(),
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                Extras = null,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice,
                DepositPrice = vehicle.DEPOSIT_AMOUNT.ToFloatNullSafe()
            } : null;
        }

        public static List<Vehicle> Map(this List<GarentaResponseBase.VEHICLE> vehicles,
            ResponseReservationStepsAdditionalInformation additionalInformation,
            List<GarentaResponseBase.ADD_PROD> addProd, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();
            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(additionalInformation, addProd, vendor));

            return _vehicles;
        }

        public static Vehicle Map(this GarentaResponseBase.STATIC_VEHICLE vehicle) =>
            vehicle != null ? new Vehicle
            {
                VendorName = "Garenta",
                VehicleCode = vehicle.NewSipp,
                VehicleName = $"{vehicle.Brand} {vehicle.Model} {vehicle.Type} ({vehicle.Fuel}-{vehicle.Transmission}) {vehicle.NewSipp}",
                SippCode = vehicle.NewSipp,
                FuelTypeName = vehicle.Fuel,
                TransmissionTypeName = vehicle.Transmission,
                DepositPrice = vehicle.Deposit.ToFloatNullSafe(),
                VendorMinimumDriverAge = vehicle.MinDriverAge.ToIntNullSafe(),
                VendorMinimumDrivingLicenseAge = vehicle.MinDriverLicenseAge.ToIntNullSafe(),
                DailyKMLimit = vehicle.DailyKmLimit.ToIntNullSafe(),
                TotalKMLimit = vehicle.TotalKmLimit.ToIntNullSafe()
            } : null;

        public static List<Vehicle> Map(this List<GarentaResponseBase.STATIC_VEHICLE> vehicles)
        {
            var _vehicles = new List<Vehicle>();
            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map());

            return _vehicles;
        }
    }
}
