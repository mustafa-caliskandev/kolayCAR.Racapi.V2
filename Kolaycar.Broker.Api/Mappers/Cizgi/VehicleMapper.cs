using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Cizgi
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this CizgiResponseBase.AvaibilityVehicleResponse vehicle,
        CizgiResponseBase cizgiResponseBase,
        ResponseReservationStepsAdditionalInformation additionalInformation,
        Vendor vendor)
        {
            int rentalDuration = cizgiResponseBase.query.duration;
            float dailyPrice = vehicle.per_day.ToFloatNullSafe();

            return vehicle != null ? new Vehicle
            {
                VehicleId = 0,
                VehicleCode = vehicle.car_id.ToString(),
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = vehicle.name.ToString(),
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = vehicle.sipp,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = rentalDuration,
                DailyPrice = dailyPrice,
                OneWayFee = vehicle.oneway_fee.ToFloatNullSafe(),
                TotalPrice = vehicle.per_day.ToFloatNullSafe() * rentalDuration,
                IsAvailable = vehicle.per_day.ToFloatNullSafe() > 0,
                VehicleImages = new List<VehicleImage> { },
                VehicleType = VehicleTypes.None,
                TransmissionType = TransmissionTypes.None,
                VehicleCategoryType = VehicleCategoryTypes.None,
                PassangerQuantityType = PassangerQuantityTypes.None,
                FuelType = FuelTypes.None,
                BaggageQuantityType = BaggageQuantityTypes.None,
                IsThereAirCondition = true,
                VendorMinimumDriverAge = vehicle.driver_age.ToIntNullSafe(),
                VendorMinimumDrivingLicenseAge = vehicle.licence_age.ToIntNullSafe(),
                DepositPrice = vehicle.deposit.ToFloatNullSafe(),
                TotalKMLimit = !string.IsNullOrEmpty(vehicle.milage_limit) ? vehicle.milage_limit.ToIntNullSafe() : (int?)null,
                DailyPricePayNow = dailyPrice,
                TotalPricePayNow = vehicle.per_day.ToFloatNullSafe() * rentalDuration,
                Extras = cizgiResponseBase.packages.Map(),
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
            }
            : null;
        }
        public static List<Vehicle> Map(this List<CizgiResponseBase.AvaibilityVehicleResponse> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, CizgiResponseBase cizgiResponseBase, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(cizgiResponseBase: cizgiResponseBase, additionalInformation: additionalInformation, vendor));

            return _vehicles;
        }

        public static Vehicle Map(this CizgiResponseBase.Fleet vehicle) =>
         vehicle != null ? new Vehicle
         {
             VendorName = "Cizgi",
             VehicleCode = vehicle.carId.ToStringNullSafe(),
             VehicleName = $"{vehicle.carName} ({vehicle.fuel}-{vehicle.transmission}-{vehicle.SIPP})",
             SippCode = vehicle.SIPP,
             FuelTypeName = vehicle.fuel,
             TransmissionTypeName = vehicle.transmission,
             VendorMinimumDriverAge = vehicle.minDriverAge.ToIntNullSafe(),
             VendorMinimumDrivingLicenseAge = vehicle.minLicenceAge.ToIntNullSafe(),
             DepositPrice = vehicle.deposit.ToFloatNullSafe()
         }
         : null;

        public static List<Vehicle> Map(this List<CizgiResponseBase.Fleet> vehicles)
        {
            var _vehicles = new List<Vehicle>();
            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map());

            return _vehicles;
        }

    }
}
