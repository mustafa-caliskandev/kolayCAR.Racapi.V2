using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.BesSResponseBase;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers._5S
{
    public static class VehiclerMapper
    {
        public static List<CommonModels.Vehicle> Map(this BesSResponseBase.VehicleClassResponse vehicleClassResponse)
        {
            var _vehicles = new List<CommonModels.Vehicle>();
            var vehicles = vehicleClassResponse.data;

            foreach (var vehicle in vehicles)
            {
                _vehicles.Add(Map(vehicle));

                //var sameVehicle = _vehicles.Where(x => x.SippCode == vehicle.sipp).ToList().Count > 0;

                //if (!sameVehicle)
                //    _vehicles.Add(Map(vehicle));
            }

            return _vehicles;
        }
        public static CommonModels.Vehicle Map(BesSResponseBase.Data vehicle)
        {
            return new CommonModels.Vehicle
            {
                VendorName = "5S",
                //VehicleId = vehicle.id.ToIntNullSafe(),
                VehicleCode = vehicle.model.id.ToStringNullSafe() + "-" + vehicle.model.vehicle_brand_id.ToStringNullSafe() + "-" + vehicle.sipp,
                VehicleName = vehicle.model.brand.name.ToStringNullSafe() + " " + vehicle.model.name.ToStringNullSafe() + " (" + vehicle.chassis_type.ToStringNullSafe() + "-" + vehicle.sipp.ToStringNullSafe() + "-" + GetFuelTypeBySippCode(vehicle.sipp.ToStringNullSafe()) + "-" + GetTransmissionTypesBySippCode(vehicle.sipp.ToStringNullSafe()) + ")",
                SippCode = vehicle.sipp.ToStringNullSafe(),
                FuelType = GetFuelTypeBySippCode(vehicle.sipp.ToStringNullSafe()),
                TransmissionType = GetTransmissionTypesBySippCode(vehicle.sipp.ToStringNullSafe())
            };
        }
        public static List<Vehicle> Map(this List<NewReponseBaseVehicleClass.Root> vehicles)
        {
            var _vehicles = new List<CommonModels.Vehicle>();
            foreach (var item in vehicles)
            {
                _vehicles.Add(Map(item));
            }
            return _vehicles;
        }
        public static Vehicle Map(this NewReponseBaseVehicleClass.Root vehicle)
        {
            return new CommonModels.Vehicle
            {
                VendorName = "5S",
                //VehicleId = vehicle.id.ToIntNullSafe(),
                VehicleCode = vehicle.model.id.ToStringNullSafe() + "-" + vehicle.model.vehicle_brand_id.ToStringNullSafe() + "-" + vehicle.sipp,
                VehicleName = vehicle.model.brand?.name?.ToStringNullSafe() + " " + vehicle.model.name.ToStringNullSafe() + " (" + vehicle.chassis_type.ToStringNullSafe() + "-" + vehicle.sipp.ToStringNullSafe() + "-" + GetFuelTypeBySippCode(vehicle.sipp.ToStringNullSafe()) + "-" + GetTransmissionTypesBySippCode(vehicle.sipp.ToStringNullSafe()) + ")",
                SippCode = vehicle.sipp.ToStringNullSafe(),
                FuelType = GetFuelTypeBySippCode(vehicle.sipp.ToStringNullSafe()),
                TransmissionType = GetTransmissionTypesBySippCode(vehicle.sipp.ToStringNullSafe())
            };
        }
        //public static List<CommonModels.Vehicle> Map(this BesSResponseBase.VehicleSearchResponse vehicleSearchResponse, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        public static List<CommonModels.Vehicle> Map(this List<BesSResponseBase.Data> vehicleSearchResponse, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<CommonModels.Vehicle>();
            var vehicles = vehicleSearchResponse;
  
            foreach (var vehicle in vehicles)
            {
                _vehicles.Add(Map(vehicle, additionalInformation, vendor));
            }

            return _vehicles;
        }
        private static CommonModels.Vehicle Map(BesSResponseBase.Data vehicle, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            float dailyPrice = Math.Round(vehicle.sale_price.ToFloatNullSafe() / vehicle.day_range.ToIntNullSafe(), 2, MidpointRounding.ToPositiveInfinity).ToFloatNullSafe();

            return new CommonModels.Vehicle
            {
                VehicleId = vehicle.id.ToIntNullSafe(),
                VehicleCode = vehicle.model.id.ToStringNullSafe() + "-" + vehicle.brand.id.ToStringNullSafe() + "-" + vehicle.sipp.ToStringNullSafe(),
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = vehicle.brand.name + " " + vehicle.model.name,
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = vehicle.sipp,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = vehicle.day_range.ToIntNullSafe(),
                DailyPrice = dailyPrice,
                OneWayFee = vehicle.drop_price.ToFloatNullSafe(),
                TotalPrice = vehicle.sale_price.ToFloatNullSafe(),
                IsAvailable = dailyPrice > 0,
                VehicleImages = new List<VehicleImage>
                {
                    new VehicleImage
                    {
                        Url = vehicle.model_image.ToStringNullSafe()
                    }
                },
                VehicleType = Get5SVehicleType(vehicle.chassis_type.ToStringNullSafe()),
                TransmissionType = Get5STransmissionTypes(vehicle.transmission.id.ToIntNullSafe()),
                VehicleCategoryType = Get5SVehicleCategoryType(vehicle.size.id),
                PassangerQuantityType = CommonModels.PassangerQuantityTypes.None,
                FuelType = Get5SFuelType(vehicle.fuel.id.ToIntNullSafe()),
                BaggageQuantityType = Get5SBaggageQuantityType(vehicle.size.baggage),
                IsThereAirCondition = true,
                VendorMinimumDriverAge = vehicle.sipp_setting != null && vehicle.sipp_setting.Count > 0 ? vehicle.sipp_setting[0].age_limit.ToIntNullSafe() : 0,
                VendorMinimumDrivingLicenseAge = vehicle.sipp_setting != null && vehicle.sipp_setting.Count > 0 ? vehicle.sipp_setting[0].license_age_limit.ToIntNullSafe() : 0,
                DailyPricePayNow = dailyPrice,
                TotalKMLimit = 0,
                TotalPricePayNow = vehicle.sale_price.ToFloatNullSafe(),
                Extras = null,
                DepositPrice = vehicle.sipp_setting != null && vehicle.sipp_setting.Count > 0 ? vehicle.sipp_setting[0].deposit.ToFloatNullSafe() : 0,
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
            };
        }
        private static BaggageQuantityTypes Get5SBaggageQuantityType(string baggageSize)
        {
            if (baggageSize == "1-2")
                return BaggageQuantityTypes.Two;
            if (baggageSize == "2-3")
                return BaggageQuantityTypes.Three;
            if (baggageSize == "3-4")
                return BaggageQuantityTypes.Four;
            if (baggageSize == "4-5")
                return BaggageQuantityTypes.Five;
            if (baggageSize == "5-6")
                return BaggageQuantityTypes.Six;
            if (baggageSize == "6-7")
                return BaggageQuantityTypes.Seven;
            if (baggageSize == "7-8")
                return BaggageQuantityTypes.Eight;
            return BaggageQuantityTypes.None;
        }
        private static VehicleCategoryTypes Get5SVehicleCategoryType(int typeId)
        {
            if (typeId == 4)
                return VehicleCategoryTypes.Economic;
            if (typeId == 5)
                return VehicleCategoryTypes.Compact;
            if (typeId == 7 || typeId == 8)
                return VehicleCategoryTypes.Intermediate;
            if (typeId == 6)
                return VehicleCategoryTypes.CompactElite;
            if (typeId == 18)
                return VehicleCategoryTypes.Premium;
            if (typeId == 15)
                return VehicleCategoryTypes.Luxury;
            if (typeId == 10)
                return VehicleCategoryTypes.Standard;
            return  VehicleCategoryTypes.None;
        }
        private static VehicleTypes Get5SVehicleType(string vehicleType)
        {
            if (vehicleType == "Sedan")
                return VehicleTypes.Sedan;
            if (vehicleType == "SUV")
                return VehicleTypes.SUV;
            if (vehicleType == "SW")
                return VehicleTypes.StationWagon;
            if (vehicleType == "HB")
                return VehicleTypes.FiveDoorHatchback;
            if (vehicleType == "VAN")
                return VehicleTypes.Van;
            return VehicleTypes.None;
        }
        private static FuelTypes Get5SFuelType(int fuelType)
        {
            if (fuelType == 3)
                return FuelTypes.Diesel;
            if (fuelType == 6)
                return FuelTypes.Gasoline;
            if (fuelType == 7)
                return FuelTypes.HybritDiesel;
            return FuelTypes.None;
        }
        private static TransmissionTypes Get5STransmissionTypes(int transmission)
        {
            if (transmission == 1)
                return TransmissionTypes.Manuel;
            if (transmission == 4)
                return TransmissionTypes.Automatic;
            return TransmissionTypes.None;
        }
        private static FuelTypes GetFuelTypeBySippCode(string sippCode)
        {
            if (sippCode.Length == 4)
            {
                var val = sippCode.ToLower()[sippCode.Length - 1];
                if (val == 'l' || val == 's')
                    return FuelTypes.GasolineAndLPG;
                if (val == 'v' || val == 'z')
                    return FuelTypes.Gasoline;
                if (val == 'd' || val == 'q')
                    return FuelTypes.Diesel;
                if (val == 'h' || val == 'i' || val == 'ı')
                    return FuelTypes.HybritGasoline;
                if (val == 'e' || val == 'c')
                    return FuelTypes.Electric;
            }
            return FuelTypes.None;
        }
        private static TransmissionTypes GetTransmissionTypesBySippCode(string sippCode)
        {
            if (sippCode.Length == 4)
            {
                var val = sippCode.ToLower()[sippCode.Length - 2];
                if (val == 'm' || val == 'n' || val == 'c')
                    return TransmissionTypes.Manuel;
                if (val == 'a' || val == 'b' || val == 'd')
                    return TransmissionTypes.Automatic;
            }
            return TransmissionTypes.None;
        }
    }
}
