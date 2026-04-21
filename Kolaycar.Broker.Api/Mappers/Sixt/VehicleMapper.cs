//using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Sixt.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Sixt
{
    public static class VehicleMapper
    {
        public static List<CommonModels.Vehicle> Map(this List<VEHICLE> apiVehicles, CommonModels.Response.ResponseReservationStepsAdditionalInformation additionalInformation, string oneWay, Vendor vendor)
        {
            var _vehicles = new List<CommonModels.Vehicle>();

            foreach (var item in apiVehicles)
            {
                if (item != null)
                    _vehicles.Add(item.Map(additionalInformation, oneWay, vendor, apiVehicles.IndexOf(item)));
            }
            _vehicles.RemoveAll(x => x == null);
            return _vehicles;
        }

        private static CommonModels.Vehicle Map(this VEHICLE apiVehicle, CommonModels.Response.ResponseReservationStepsAdditionalInformation additionalInformation, string oneWay, Vendor vendor, int index = 1)
        {
            if (apiVehicle.INCLUDED != null)
            {
                var kmLimit = apiVehicle.INCLUDED.INCLUDE.FirstOrDefault(x => x is { CODE: "SK_400" })?.NAME.ToIntNullSafe();
                var rentalDuration = (int)Math.Ceiling((additionalInformation.ReturnDateTime - additionalInformation.PickupDateTime).TotalDays);

                return new CommonModels.Vehicle
                {
                    VehicleId = index,
                    VehicleCode = apiVehicle.GROUPNAME,
                    VendorId = additionalInformation.Vendor.VendorId,
                    VendorName = additionalInformation.Vendor.VendorName,
                    VendorPhone = additionalInformation.Vendor.VendorPhone,
                    VendorEmail = additionalInformation.Vendor.VendorEmail,
                    VehicleName = $"{apiVehicle.SAMPLECARTITLES} {apiVehicle.PROPERTIES.FUELTYPE} - {apiVehicle.PROPERTIES.GEAR}",
                    VendorLogo = additionalInformation.Vendor.Logo,
                    PickupLocationId = additionalInformation.PickupLocationId,
                    PickupLocationName = additionalInformation.PickupLocationName,
                    ReturnLocationId = additionalInformation.ReturnLocationId,
                    ReturnLocationName = additionalInformation.ReturnLocationName,
                    PickupDateTime = additionalInformation.PickupDateTime,
                    ReturnDateTime = additionalInformation.ReturnDateTime,
                    RentalDuration = rentalDuration,
                    DailyPrice = apiVehicle.DAILYPRICE.ToFloatNullSafe(),
                    OneWayFee = oneWay.ToFloatNullSafe(),
                    TotalPrice = apiVehicle.DAILYPRICE.ToFloatNullSafe() * additionalInformation.RentalDuration.ToFloatNullSafe(),
                    IsAvailable = apiVehicle.AVAILABILITY.STATUS.Contains("1"),
                    VehicleImages = new List<CommonModels.VehicleImage>
                {
                    new CommonModels.VehicleImage
                    {
                        Url = apiVehicle.PICTURE.ToStringNullSafe()
                    }
                },
                    VehicleType = GetVehicleType(apiVehicle.PROPERTIES.TYPE),
                    TransmissionType = GetTransmissionType(apiVehicle.PROPERTIES.GEAR),
                    VehicleCategoryType = GetVehicleCategoryType(apiVehicle.PROPERTIES.CLASS),
                    PassangerQuantityType = (CommonModels.PassangerQuantityTypes)apiVehicle.PROPERTIES.PASSENGERS.ToIntNullSafe(),
                    FuelType = GetFuelType(apiVehicle.PROPERTIES.FUELTYPE),
                    BaggageQuantityType = CommonModels.BaggageQuantityTypes.None,
                    IsThereAirCondition = apiVehicle.PROPERTIES.AIRCONDITION == "1" || apiVehicle.PROPERTIES.AIRCONDITION.Contains("1"),
                    VendorMinimumDriverAge = apiVehicle.AGELIMIT.ToIntNullSafe(),
                    VendorMinimumDrivingLicenseAge = apiVehicle.DRIVERLICANSELIMIT.ToIntNullSafe(),
                    DailyPricePayNow = apiVehicle.DAILYPRICE.ToFloatNullSafe(),
                    TotalPricePayNow = apiVehicle.DAILYPRICE.ToFloatNullSafe() * additionalInformation.RentalDuration.ToFloatNullSafe(),
                    DepositPrice = apiVehicle.DEPOSIT.ToFloatNullSafe(),
                    DailyKMLimit = (kmLimit / rentalDuration).ToIntNullSafe(),
                    TotalKMLimit = kmLimit,
                    FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                    Extras = apiVehicle.Map(),
                    RentalWorkingTypes = vendor.RentalWorkingType,
                    ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
                };
            }
            else
            {
                var rentalDuration = (int)Math.Ceiling((additionalInformation.ReturnDateTime - additionalInformation.PickupDateTime).TotalDays);

                return new CommonModels.Vehicle
                {
                    VehicleId = index,
                    VehicleCode = apiVehicle.GROUPNAME,
                    VendorId = additionalInformation.Vendor.VendorId,
                    VendorName = additionalInformation.Vendor.VendorName,
                    VendorPhone = additionalInformation.Vendor.VendorPhone,
                    VendorEmail = additionalInformation.Vendor.VendorEmail,
                    VehicleName = $"{apiVehicle.SAMPLECARTITLES} {apiVehicle.PROPERTIES.FUELTYPE} - {apiVehicle.PROPERTIES.GEAR}",
                    VendorLogo = additionalInformation.Vendor.Logo,
                    PickupLocationId = additionalInformation.PickupLocationId,
                    PickupLocationName = additionalInformation.PickupLocationName,
                    ReturnLocationId = additionalInformation.ReturnLocationId,
                    ReturnLocationName = additionalInformation.ReturnLocationName,
                    PickupDateTime = additionalInformation.PickupDateTime,
                    ReturnDateTime = additionalInformation.ReturnDateTime,
                    RentalDuration = rentalDuration,
                    DailyPrice = apiVehicle.DAILYPRICE.ToFloatNullSafe(),
                    OneWayFee = oneWay.ToFloatNullSafe(),
                    TotalPrice = apiVehicle.DAILYPRICE.ToFloatNullSafe() * additionalInformation.RentalDuration.ToFloatNullSafe(),
                    IsAvailable = apiVehicle.AVAILABILITY.STATUS.Contains("1"),
                    VehicleImages = new List<CommonModels.VehicleImage>
                {
                    new CommonModels.VehicleImage
                    {
                        Url = apiVehicle.PICTURE.ToStringNullSafe()
                    }
                },
                    VehicleType = GetVehicleType(apiVehicle.PROPERTIES.TYPE),
                    TransmissionType = GetTransmissionType(apiVehicle.PROPERTIES.GEAR),
                    VehicleCategoryType = GetVehicleCategoryType(apiVehicle.PROPERTIES.CLASS),
                    PassangerQuantityType = (CommonModels.PassangerQuantityTypes)apiVehicle.PROPERTIES.PASSENGERS.ToIntNullSafe(),
                    FuelType = GetFuelType(apiVehicle.PROPERTIES.FUELTYPE),
                    BaggageQuantityType = CommonModels.BaggageQuantityTypes.None,
                    IsThereAirCondition = apiVehicle.PROPERTIES.AIRCONDITION == "1" || apiVehicle.PROPERTIES.AIRCONDITION.Contains("1"),
                    VendorMinimumDriverAge = apiVehicle.AGELIMIT.ToIntNullSafe(),
                    VendorMinimumDrivingLicenseAge = apiVehicle.DRIVERLICANSELIMIT.ToIntNullSafe(),
                    DailyPricePayNow = apiVehicle.DAILYPRICE.ToFloatNullSafe(),
                    TotalPricePayNow = apiVehicle.DAILYPRICE.ToFloatNullSafe() * additionalInformation.RentalDuration.ToFloatNullSafe(),
                    DepositPrice = apiVehicle.DEPOSIT.ToFloatNullSafe(),
                    DailyKMLimit = 0,
                    TotalKMLimit = 0,
                    FullCredit = false,
                    Extras = apiVehicle.Map()
                };
            }
            return null;

        }

        public static List<CommonModels.Vehicle> Map(this List<VEHICLE> apiVehicles)
        {
            var _vehicles = new List<CommonModels.Vehicle>();

            int i = 1;
            foreach (var vehicle in apiVehicles)
            {
                if (vehicle != null)
                {
                    _vehicles.Add(vehicle.Map(i));
                    i++;
                }
            }

            return _vehicles;
        }

        private static CommonModels.Vehicle Map(this VEHICLE apiVehicle, int i)
        {
            return new CommonModels.Vehicle
            {
                VehicleId = i,
                VehicleCode = apiVehicle.GROUPNAME.ToUpper(),
                VehicleName = apiVehicle.SAMPLECARTITLES + " " + apiVehicle.PROPERTIES.FUELTYPE + " " + apiVehicle.PROPERTIES.GEAR,
                SippCode = apiVehicle.GROUPNAME,
                FuelType = GetFuelType(apiVehicle.PROPERTIES.FUELTYPE),
                TransmissionType = GetTransmissionType(apiVehicle.PROPERTIES.GEAR),
                VehicleCategoryType = GetVehicleCategoryType(apiVehicle.PROPERTIES.TYPE)
            };
        }

        private static CommonModels.FuelTypes GetFuelType(string fuelType)
        {
            return fuelType.Contains("Benzin") ? CommonModels.FuelTypes.Gasoline : fuelType.Contains("Elektrik") ? CommonModels.FuelTypes.Electric : fuelType == "" ? CommonModels.FuelTypes.None : CommonModels.FuelTypes.Diesel;
        }

        private static CommonModels.TransmissionTypes GetTransmissionType(string gear)
        {
            return gear.Contains("Manuel") ? CommonModels.TransmissionTypes.Manuel : gear.Contains("Otomatik") ? CommonModels.TransmissionTypes.Automatic : CommonModels.TransmissionTypes.None;
        }

        private static CommonModels.VehicleCategoryTypes GetVehicleCategoryType(string type)
        {
            if (type == "Suv")
                return CommonModels.VehicleCategoryTypes.SUV;
            if (type == "Üst")
                return CommonModels.VehicleCategoryTypes.Luxury;
            if (type == "Orta")
                return CommonModels.VehicleCategoryTypes.Standard;
            if (type == "Ekonomik")
                return CommonModels.VehicleCategoryTypes.Economic;
            return CommonModels.VehicleCategoryTypes.None;
        }

        private static CommonModels.VehicleTypes GetVehicleType(string type)
        {
            if (type == "SUV")
                return CommonModels.VehicleTypes.SUV;
            if (type == "Binek")
                return CommonModels.VehicleTypes.Sedan;
            return CommonModels.VehicleTypes.None;
        }

    }
}
