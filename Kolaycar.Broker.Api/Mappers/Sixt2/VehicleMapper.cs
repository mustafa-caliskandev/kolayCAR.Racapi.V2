using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Mappers.Sixt2
{
    public static class VehicleMapper
    {
        public static List<Vehicle> Map(this List<SixtAvailableVehicle> vehicles, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation)
        {

            if (vehicles == null || vehicles.Count == 0)
                return new List<Vehicle>();

            var mappedVehicles = new List<Vehicle>();

            foreach (var vehicle in vehicles)
            {
                var vehicleCodes = vehicle.vehicle_brands
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(brand => $"{vehicle.vehicle_group}|{brand.Replace(" ", "")}")
                    .ToList();

                foreach (var code in vehicleCodes)
                {
                    if (mappedVehicles.Any(e => e.VehicleCode.Equals(code, StringComparison.InvariantCultureIgnoreCase)))
                        continue;

                    mappedVehicles.Add(new Vehicle
                    {
                        VehicleId = 0,
                        VehicleCode = code.ToUpperInvariant().Replace("İ", "I"),
                        VendorId = vendor.VendorId,
                        VendorName = vendor.VendorName,
                        VendorPhone = vendor.VendorPhone,
                        VendorEmail = vendor.VendorEmail,
                        VendorLogo = vendor.Logo,
                        VehicleName = vehicle.vehicle_brands,
                        PickupLocationId = additionalInformation.PickupLocationId,
                        PickupLocationName = additionalInformation.PickupLocationName,
                        ReturnLocationId = additionalInformation.ReturnLocationId,
                        ReturnLocationName = additionalInformation.ReturnLocationName,
                        PickupDateTime = additionalInformation.PickupDateTime,
                        ReturnDateTime = additionalInformation.ReturnDateTime,
                        RentalDuration = vehicle.rental_day,
                        DailyPrice = vehicle.daily_price.ToFloatNullSafe(),
                        OneWayFee = vehicle.one_way_price.ToFloatNullSafe(),
                        TotalPrice = vehicle.daily_total_price.ToFloatNullSafe(),
                        IsAvailable = vehicle.vehicle_available is "UYGUN" or "FREE SALE",
                        VehicleType = VehicleTypes.None,
                        TransmissionType = GetTransmissionType(vehicle.vehicle_features.gear),
                        VehicleCategoryType = VehicleCategoryTypes.None,
                        PassangerQuantityType = PassangerQuantityTypes.None,
                        FuelType = GetFuelType(vehicle.vehicle_features.fuel),
                        BaggageQuantityType = BaggageQuantityTypes.None,
                        VendorMinimumDriverAge = vehicle.vehicle_features.age,
                        VendorMinimumDrivingLicenseAge = vehicle.vehicle_features.min_license_age,
                        DailyPricePayNow = vehicle.daily_price.ToFloatNullSafe(),
                        TotalPricePayNow = vehicle.daily_total_price.ToFloatNullSafe(),
                        DepositPrice = vehicle.vehicle_features.deposit_amount.ToFloatNullSafe(),
                        DailyKMLimit = (vehicle.max_km / vehicle.rental_day).ToIntNullSafe(),
                        TotalKMLimit = vehicle.max_km,
                        FullCredit = vendor.CreditType == CreditType.FullCredit,
                        RentalWorkingTypes = vendor.RentalWorkingType,
                        ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
                    });
                }
            }

            return mappedVehicles;
        }
        public static List<Vehicle> Map(this List<SixtVehicle> vehicles, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();
            foreach (var vehicle in vehicles)
            {
                string vehicleCode = vehicle.vehicle_group + "|" + vehicle.brand_model.Replace(" ", "");
                if (!_vehicles.Any(v => v.VehicleCode == vehicleCode))
                {
                    var mappedVehicle = new Vehicle
                    {
                        VendorName = vendor.VendorName,
                        VehicleCode = StringHelper.ToTurkishCharacterEscapeUpperCase(vehicleCode),
                        VehicleName = $"({vehicle.brand_model} {GetFuelName(vehicle.fuel_type, vehicle.vehicle_group)}-{GetTransmissionName(vehicle.gear_types, vehicle.vehicle_group)})",
                        SippCode = vehicle.vehicle_group,
                        FuelTypeName = GetFuelName(vehicle.fuel_type, vehicle.vehicle_group),
                        TransmissionTypeName = GetTransmissionName(vehicle.gear_types, vehicle.vehicle_group),
                        VendorMinimumDriverAge = vehicle.age_limit.ToIntNullSafe()
                    };
                    _vehicles.Add(mappedVehicle);
                }
            }

            return _vehicles;
        }
        public static string GetFuelName(string fuelType, string sippCode)
        {
            string fuel;
            if (fuelType.Contains(","))
            {
                if (sippCode.Length > 3)
                {
                    return sippCode[3] switch
                    {
                        'D' => "Dizel",
                        'R' => "Benzin",
                        'E' => "Elektrik",
                        _ => "Vites bilgisi yok"
                    };
                }
            }
            switch (fuelType)
            {
                case "1": fuel = "Benzin"; break;
                case "2": fuel = "Dizel"; break;
                case "3": fuel = "LPG"; break;
                case "4": fuel = "Elektrik"; break;
                case "5": fuel = "Hibrit"; break;
                default: fuel = "Yakıt bilgisi yok"; break;
            }
            ;
            return fuel;
        }
        public static FuelTypes GetFuelType(int fuelType)
        {
            FuelTypes fuelTypes;

            switch (fuelType)
            {
                case 1: fuelTypes = FuelTypes.Gasoline; break;
                case 2: fuelTypes = FuelTypes.Diesel; break;
                case 3: fuelTypes = FuelTypes.GasolineAndLPG; break;
                case 4: fuelTypes = FuelTypes.Electric; break;
                case 5: fuelTypes = FuelTypes.Hybrid; break;
                default: fuelTypes = FuelTypes.None; break;
            }
           ;
            return fuelTypes;
        }
        public static string GetTransmissionName(string transmissionType, string sippCode)
        {
            if (transmissionType.Contains(","))
            {
                if (sippCode.Length > 3)
                {
                    return sippCode.ElementAt(2) switch
                    {
                        'A' => "Otomatik",
                        'M' => "Manuel",
                        _ => "Vites bilgisi yok"
                    };
                }
            }
            string transmission;
            switch (transmissionType)
            {
                case "0": transmission = "Otomatik"; break;
                case "1": transmission = "Manuel"; break;
                default: transmission = "Vites bilgisi yok"; break;
            }
            ;
            return transmission;
        }
        public static TransmissionTypes GetTransmissionType(int type)
        {
            return type switch
            {
                0 => TransmissionTypes.Automatic,
                1 => TransmissionTypes.Manuel,
                _ => TransmissionTypes.None
            };
        }

    }
}
