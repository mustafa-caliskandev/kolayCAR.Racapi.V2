using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.WheelsysResponseBase;

namespace KolayCAR.Broker.API.Mappers.Wheelsys
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this Category vehicle,
            Pricequote pricequote,
            ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            float totalPrice = VehicleHelper.ConvertCommaFreePriceToFloat(vehicle.totalrate);
            float oneWayFee = VehicleHelper.ConvertCommaFreePriceToFloat(vehicle.onewaycharge);
            float provision = VehicleHelper.ConvertCommaFreePriceToFloat(vehicle.provision);

            int rentalDuration = Convert.ToInt32(pricequote.duration ?? "1");

            float dailyPrice = (totalPrice - oneWayFee) / rentalDuration;

            return vehicle != null ? new Vehicle
            {
                VehicleId = 0,
                VehicleCode = vehicle.code,
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = vehicle.model,
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
                OneWayFee = oneWayFee,
                TotalPrice = totalPrice,
                IsAvailable = vehicle.availability == "AVAILABLE",
                VehicleImages = null,
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
                TotalPricePayNow = totalPrice,
                DepositPrice = provision,
                Extras = vehicle.options?.option.Map(vendor),
                TotalKMLimit = vehicle.klmincluded.ToIntNullSafe(),
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
            }
            : null;
        }

        public static List<Vehicle> Map(this List<Category> vehicles,
            Pricequote pricequote,
            ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(pricequote: pricequote, additionalInformation: additionalInformation, vendor));

            return _vehicles;
        }
        public static Vehicle Map(this WheelsysResponseBase.Cargroup vehicle, Vendor vendor) =>
               vehicle != null ? new Vehicle
               {
                   VendorName = vendor.VendorName,
                   VehicleCode = vehicle.code,
                   VehicleName = vehicle.model,
                   SippCode = vehicle.sippcode
               }
               : null;
        public static List<Vehicle> Map(this List<WheelsysResponseBase.Cargroup> vehicles, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(vendor));

            return _vehicles;
        }


    }
}
