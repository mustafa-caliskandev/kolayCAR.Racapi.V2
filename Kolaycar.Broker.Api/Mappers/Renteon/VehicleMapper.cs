using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Renteon
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this RenteonResponseBase.CarCategory vehicle, Vendor vendor) =>
            vehicle != null ? new Vehicle
            {
                VendorName = vendor.VendorName,
                VehicleCode = vehicle.Code,
                SippCode = vehicle.Code,
                VehicleName = vehicle.Code,
            }
            : null;

        public static List<Vehicle> Map(this List<RenteonResponseBase.CarCategory> vehicles, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(vendor));

            return _vehicles;
        }

        public static Vehicle Map(
            this RenteonResponseBase.RenteonVehicleResponseBase vehicle,
            ResponseReservationStepsAdditionalInformation additionalInformation,
            int duration, Vendor vendor) =>
            vehicle != null ? new Vehicle
            {
                VehicleCode = vehicle.CarCategory,
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = vehicle.ModelName,
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = vehicle.CarCategory,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = additionalInformation.RentalDuration == 0 ? duration : additionalInformation.RentalDuration,
                DailyPrice = (float)(vehicle.Amount / (additionalInformation.RentalDuration.ToIntNullSafe() == 0 ? duration : additionalInformation.RentalDuration.ToIntNullSafe())),
                TotalPrice = (float)vehicle.Amount,
                PassangerQuantityName = vehicle.PassengerCapacity.ToStringNullSafe(),
                VendorMinimumDriverAge = vehicle.MinimumDriverAge,
                Extras = GetExtras(vehicle),
                IsAvailable = true,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
            }
            : null;

        public static List<Vehicle> Map(
            this List<RenteonResponseBase.RenteonVehicleResponseBase> vehicles,
            ResponseReservationStepsAdditionalInformation additionalInformation,
            int duration,Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(additionalInformation, duration, vendor));

            return _vehicles;
        }

        public static List<Domain.Models.Extra> GetExtras(RenteonResponseBase.RenteonVehicleResponseBase vehicle)
        {
            List<Domain.Models.Extra> vehicleExtras = new List<Domain.Models.Extra>();
            foreach (var extra in vehicle.AvailableServices)
            {
                vehicleExtras.Add(new Domain.Models.Extra
                {
                    ExtraCode = extra.Code,
                    Price = (float)extra.Amount,
                    ExtraRentalType = ExtraRentalTypes.PerRental,
                    ExtraId = 0
                });
            }
            return vehicleExtras;
        }
    }
}
