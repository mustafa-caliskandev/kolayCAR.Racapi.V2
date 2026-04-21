using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Erboycar
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this ErboycarResponseBase.AvailableVehicle vehicle,
          ErboycarResponseBase.VehicleResponse erboycarResponseBase,
          ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            int rentalDuration = erboycarResponseBase.rentalDays;

            return vehicle != null ? new Vehicle
            {
                VehicleId = 0,
                VehicleCode = vehicle._id.ToStringNullSafe(),
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = vehicle.model.ToStringNullSafe(),
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = vehicle.group,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = rentalDuration,
                DailyPrice = vehicle.price.ToFloatNullSafe(),
                OneWayFee = erboycarResponseBase.ToFloatNullSafe(),
                TotalPrice = (vehicle.price * erboycarResponseBase.rentalDays).ToFloatNullSafe(),
                IsAvailable = vehicle.status?.ToLower() == "available",
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
                VendorMinimumDriverAge = vehicle.driver_age.ToIntNullSafe(),
                VendorMinimumDrivingLicenseAge = vehicle.min_licence_age.ToIntNullSafe(),
                DepositPrice = vehicle.deposit.ToFloatNullSafe(),
                TotalKMLimit = !string.IsNullOrEmpty(vehicle.max_km) ? vehicle.max_km.ToIntNullSafe() : (int?)null,
                DailyPricePayNow = vehicle.price.ToFloatNullSafe(),
                TotalPricePayNow = (vehicle.price * erboycarResponseBase.rentalDays).ToFloatNullSafe(),
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                Extras = null,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
            }
            : null;
        }
        public static List<Vehicle> Map(this List<ErboycarResponseBase.AvailableVehicle> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, ErboycarResponseBase.VehicleResponse erboycarResponseBase, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(erboycarResponseBase: erboycarResponseBase, additionalInformation: additionalInformation, vendor));

            return _vehicles;
        }

        public static Vehicle Map(this ErboycarResponseBase.VehicleListItem vehicle) =>
         vehicle != null ? new Vehicle
         {
             VendorName = "Erboycar",
             VehicleCode = vehicle._id.ToStringNullSafe(),
             VehicleName = $"{vehicle.similar_vehicle} ({vehicle.fuel_type}-{vehicle.transmission_type})",
             SippCode = vehicle.group,
             FuelTypeName = vehicle.fuel_type,
             TransmissionTypeName = vehicle.transmission_type,
             VendorMinimumDriverAge = vehicle.driver_age.ToIntNullSafe(),
             VendorMinimumDrivingLicenseAge = vehicle.min_licence_age.ToIntNullSafe(),
             DepositPrice = vehicle.deposit.ToFloatNullSafe()
         }
         : null;

        public static List<Vehicle> Map(this List<ErboycarResponseBase.VehicleListItem> vehicles)
        {
            var _vehicles = new List<Vehicle>();
            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map());

            return _vehicles;
        }
    }
}
