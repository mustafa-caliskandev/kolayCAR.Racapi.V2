using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Europcar
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this category vehicle,
            pricequote pricequote,
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
                Extras = vehicle.options?.option.Map(),
                DailyKMLimit = vehicle.klmincluded.ToIntNullSafe(),
                TotalKMLimit = vehicle.klmTotal.ToIntNullSafe(),
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
            }
            : null;
        }

        public static List<Vehicle> Map(this List<category> vehicles,
            pricequote pricequote,
            ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(pricequote: pricequote, additionalInformation: additionalInformation, vendor));

            return _vehicles;
        }

        public static Vehicle Map(this cargroup vehicle) =>
            vehicle != null ? new Vehicle
            {
                VendorName = "Europcar",
                VehicleCode = vehicle.code,
                //VehicleName = vehicle.model + " - " + vehicle.code,
                SippCode = vehicle.code,
                VehicleName = vehicle.model + " - " + vehicle.code + " - " + vehicle.gear,
                TransmissionTypeName = vehicle.gear,
                TransmissionType = GetTransmissionTypes(vehicle.gear),
                VendorMinimumDriverAge = vehicle.driverMinYear,
                VendorMinimumDrivingLicenseAge = vehicle.licenseMinYear,
                VehicleTypeName = vehicle.category,
                VehicleType = GetVehicleType(vehicle.category)

            }
            : null;
        public static VehicleTypes GetVehicleType(string category)
        {
            switch (category)
            {
                case "HCB": return VehicleTypes.FiveDoorHatchback;
                case "SV": return VehicleTypes.SUV;
                case "SDN": return VehicleTypes.Sedan;
                case "Karavan": return VehicleTypes.Van;
                case "MNB": return VehicleTypes.Van;
                case "Eco-SUV<": return VehicleTypes.SUV;
                case "MPV": return VehicleTypes.Van;
                case "STW": return VehicleTypes.StationWagon;
                case "Commercial": return VehicleTypes.Van;
                default: return VehicleTypes.None;
            }
        }
        public static TransmissionTypes GetTransmissionTypes(string gear)
        {
            switch (gear)
            {
                case "Manuel": return TransmissionTypes.Manuel;
                case "Otomatik": return TransmissionTypes.Automatic;
                default: return TransmissionTypes.None;
            }
        }
        public static List<Vehicle> Map(this List<cargroup> vehicles)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map());

            return _vehicles;
        }
        public static List<Vehicle> Map(this List<EuropcarVehicle> vehicles, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(vendor));

            return _vehicles;
        }
        public static Vehicle Map(this EuropcarVehicle vehicle, Vendor vendor) =>
           vehicle != null ? new Vehicle
           {
               VendorName = vendor.VendorName,
               VehicleCode = vehicle.groupCode,
               SippCode = vehicle.groupCode,
               VehicleName = vehicle.groupName + " - " + vehicle.fuelType + " - " + vehicle.gearType,
               TransmissionTypeName = vehicle.gearType,
               TransmissionType = GetTransmissionTypes(vehicle.gearType),
               VendorMinimumDriverAge = vehicle.minimumDriverAge,
               VendorMinimumDrivingLicenseAge = vehicle.minimumLicenseAge,
               DepositPrice = vendor.CurrencyType == CurrencyTypes.TRY 
                                ? vehicle.tryProvisionAmount.ToFloatNullSafe() 
                                : vendor.CurrencyType == CurrencyTypes.EUR  
                                        ? vehicle.euroProvisionAmount.ToFloatNullSafe()
                                        : vehicle.tryProvisionAmount.ToFloatNullSafe()
           }
           : null;
    }
}
