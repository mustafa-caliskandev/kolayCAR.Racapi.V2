using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using System.Linq;
using static KolayCAR.Broker.Domain.Models.Response.PandoraResponseBase;

namespace KolayCAR.Broker.API.Mappers.Pandora
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this AvailableVehicle vehicle,
            ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            //double dayDifference = (additionalInformation.ReturnDateTime - additionalInformation.PickupDateTime).TotalDays;
            //int rentalDuration = dayDifference > (int)dayDifference &&
            //    (additionalInformation.ReturnDateTime.TimeOfDay - additionalInformation.PickupDateTime.TimeOfDay).TotalHours >= 2 ?
            //    (int)dayDifference + 1 : (int)dayDifference;

            int rentalDuration = vehicle.DaysForPayment;
            float oneWayFee = vehicle.IncludedServices?
                .FirstOrDefault(x => x.ServiceTypeId == 10)?
                .AmountTotal
                .ToFloatNullSafe() ?? 0f;
            float deliveryFee = vehicle.IncludedServices?
                .Where(x => x.ServiceTypeId == 13)?
                .Sum(x => x.AmountTotal)
                .ToFloatNullSafe() ?? 0f;

            float dailyPrice = (vehicle.Amount.ToFloatNullSafe() - oneWayFee) / rentalDuration;

            return vehicle != null ? new Vehicle
            {
                VehicleId = vehicle.CarCategoryId,
                VehicleCode = vehicle.CarCategoryId.ToString(),
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = vehicle.ModelName,
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = vehicle.SIPP,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = vehicle.DaysForPayment,
                DailyPrice = dailyPrice,
                OneWayFee = oneWayFee + deliveryFee,
                TotalPrice = vehicle.Amount.ToFloatNullSafe(),
                IsAvailable = vehicle.Amount > 0,
                VehicleImages = new List<VehicleImage> { },
                VehicleType = VehicleTypes.None,
                TransmissionType = TransmissionTypes.None,
                VehicleCategoryType = VehicleCategoryTypes.None,
                PassangerQuantityType = PassangerQuantityTypes.None,
                FuelType = FuelTypes.None,
                BaggageQuantityType = BaggageQuantityTypes.None,
                IsThereAirCondition = true,
                VendorMinimumDriverAge = 21,
                VendorMinimumDrivingLicenseAge = 2,
                DailyPricePayNow = dailyPrice,
                TotalPricePayNow = vehicle.Amount.ToFloatNullSafe(),
                Extras = vehicle.AvailableServices?.Map(),
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
            }
            : null;
        }

        public static List<Vehicle> Map(this List<AvailableVehicle> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(additionalInformation: additionalInformation, vendor));

            return _vehicles;
        }

        public static Vehicle Map(this CarCategories vehicle) =>
            vehicle != null ? new Vehicle
            {
                VendorName = "Pandora",
                VehicleId = vehicle.Id,
                VehicleCode = vehicle.Id.ToString(),
                VehicleName = $"{vehicle.CarModel} - {string.Join('&', vehicle.FuelTypes.Select(x => x.Name))} - {vehicle.CarTransmissionType.Name} - {vehicle.SIPP}",
                SippCode = vehicle.SIPP,
                FuelTypeName = string.Join('&', vehicle.FuelTypes.Select(x => x.Name)),
                TransmissionTypeName = vehicle.CarTransmissionType.Name,
                DepositPrice = vehicle.DepositAmount
            }
            : null;

        public static List<Vehicle> Map(this List<CarCategories> vehicles)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map());

            return _vehicles;
        }
    }
}
