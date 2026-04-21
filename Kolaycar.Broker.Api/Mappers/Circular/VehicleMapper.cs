using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Mappers.Circular
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this CircularResponseBase.Group vehicle,
        ResponseReservationStepsAdditionalInformation additionalInformation,Vendor vendor,
        string baseUrl = "",
        float dropPrice = 0)
        {
            int rentalDuration = Math.Ceiling((additionalInformation.ReturnDateTime - additionalInformation.PickupDateTime).TotalDays).ToString().ToIntNullSafe();
            float dailyPrice = vehicle.dateranges.First().periods.First().dailyprice.ToFloatNullSafe();
            var total = vehicle.dateranges.First().periods.FirstOrDefault().totalprice.ToFloatNullSafe();
          
            return vehicle != null ? new Vehicle
            {
                VehicleId = vehicle.id,
                VehicleCode = vehicle.id.ToString(),
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = vehicle.examplemodel.ToString(),
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = vehicle.title,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = rentalDuration,
                DailyPrice = dailyPrice,
                OneWayFee = dropPrice, // Eklenecek
                TotalPrice = total,
                IsAvailable = true,//vehicle.dateranges.First().periods.FirstOrDefault().totalprice.ToFloatNullSafe() > 0,
                VehicleImages = new List<VehicleImage> { new VehicleImage { Url = baseUrl + "/" + vehicle.photo.url } },
                VehicleType = VehicleTypes.None,
                TransmissionType = TransmissionTypes.None,
                VehicleCategoryType = VehicleCategoryTypes.None,
                PassangerQuantityType = PassangerQuantityTypes.None,
                FuelType = FuelTypes.Diesel,
                BaggageQuantityType = BaggageQuantityTypes.None,
                IsThereAirCondition = true,
                VendorMinimumDriverAge = vehicle.driverminage.ToIntNullSafe(),
                VendorMinimumDrivingLicenseAge = 2,//vehicle.dri.ToIntNullSafe(),
                DepositPrice = vehicle.securitydeposit.ToFloatNullSafe(),
                TotalKMLimit = !string.IsNullOrEmpty(vehicle.mountlykmlimit) ? vehicle.mountlykmlimit.ToIntNullSafe() : (int?)null,
                DailyPricePayNow = dailyPrice,
                TotalPricePayNow = total,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice,
                // Extras = CircularResponseBase.packages.Map()
                Extras = new List<Extra> { }
            }
            : null;
        }
        //public static List<Vehicle> Map(this List<CircularResponseBase.AvaibilityVehicleResponse> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, CircularResponseBase CircularResponseBase)
        //{
        //    var _vehicles = new List<Vehicle>();

        //    if (vehicles != null && vehicles.Count != 0)
        //        foreach (var vehicle in vehicles)
        //            _vehicles.Add(vehicle.Map(CircularResponseBase: CircularResponseBase, additionalInformation: additionalInformation));

        //    return _vehicles;
        //}

        public static Vehicle Map(this CircularResponseBase.Group vehicle) =>
         vehicle != null ? new Vehicle
         {
             VendorName = "Circular",
             VehicleCode = vehicle.id.ToStringNullSafe(),
             VehicleName = $"{vehicle.examplemodel} ({vehicle.fuelandac}-{vehicle.transmissionanddrive})",
             SippCode = vehicle.title,
             FuelTypeName = vehicle.fuelandac,
             TransmissionTypeName = vehicle.transmissionanddrive,
             VendorMinimumDriverAge = vehicle.driverminage.ToIntNullSafe(),
             //VendorMinimumDrivingLicenseAge = vehicle.dri.ToIntNullSafe(),
             DepositPrice = vehicle.securitydeposit.ToFloatNullSafe(),
             IsAvailable = true,
         }
         : null;

        public static List<Vehicle> Map(this CircularResponseBase.AvaibilityVehicleResponse data, ResponseReservationStepsAdditionalInformation additionalInformation,Vendor vendor, string baseUrl = "")
        {
            var _vehicles = new List<Vehicle>();
            foreach (var item in data.location.groups)
            {
                _vehicles.Add(item.Map(additionalInformation,vendor, baseUrl: baseUrl, dropPrice: data.location.reservation.dropprice.ToFloatNullSafe()));
            }
            return _vehicles;
        }

        public static List<Vehicle> Map(this CircularResponseBase.VehicleList data)
        {
            var _vehicles = new List<Vehicle>();
            foreach (var item in data.list)
            {
                _vehicles.Add(item.Map());

            }
            return _vehicles;
        }

    }
}
