using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Elitcar
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this ElitcarResponseBase.List vehicle,
          ElitcarResponseBase elitcarResponseBase,
          ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            int rentalDuration = elitcarResponseBase.options.day;
            float dailyPrice = vehicle.price / rentalDuration;

            return vehicle != null ? new Vehicle
            {
                VehicleId = 0,
                VehicleCode = vehicle.id.ToString(),
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = vehicle.id.ToString(),
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = vehicle.sipp_code,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = rentalDuration,
                DailyPrice = dailyPrice,
                OneWayFee = 0,
                TotalPrice = vehicle.price,
                IsAvailable = vehicle.price > 0,
                VehicleImages = new List<VehicleImage>
                {
                    new VehicleImage{ Url = vehicle.image_url }
                },
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
                TotalPricePayNow = vehicle.price,
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice,
                Extras = null
            }
            : null;
        }
        public static List<Vehicle> Map(this List<ElitcarResponseBase.List> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, ElitcarResponseBase elitcarResponseBase, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(elitcarResponseBase: elitcarResponseBase, additionalInformation: additionalInformation, vendor));

            return _vehicles;
        }

        public static Vehicle Map(this ElitcarResponseBase.Vehicle vehicle) =>
         vehicle != null ? new Vehicle
         {
             VendorName = "Elitcar",
             VehicleCode = vehicle.id.ToStringNullSafe(),
             VehicleName = $"{vehicle.brand} {vehicle.name} ({vehicle.fuel}-{vehicle.gear})",
             SippCode = vehicle.sipp_code,
             FuelTypeName = vehicle.fuel,
             TransmissionTypeName = vehicle.gear
         }
         : null;

        public static List<Vehicle> Map(this List<ElitcarResponseBase.Vehicle> vehicles)
        {
            var _vehicles = new List<Vehicle>();
            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map());

            return _vehicles;
        }


    }
}
