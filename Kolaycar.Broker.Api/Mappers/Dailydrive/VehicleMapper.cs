using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Response.Dailydrive;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Mappers.Dailydrive
{
    public static class VehicleMapper
    {
        public static List<Vehicle> Map(this List<Ns2VehicleType> vehicles, List<Ns2VehicleClasses> ns2VehicleClasses = null)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)

                foreach (var vehicleType in vehicles)
                {
                    _vehicles.Add(vehicleType.Map(ns2VehicleClasses?.Where(e => e.Ns2ClassNo == vehicleType.ns2classNo).FirstOrDefault()));
                }
            return _vehicles;
        }
        public static Vehicle Map(this Ns2VehicleType vehicle, Ns2VehicleClasses ns2VehicleClass = null) =>
     vehicle != null ? new Vehicle
     {
         VendorName = "Dailydrive",
         //VehicleId = Convert.ToInt32(vehicle.Ns2ClassNo),
         VehicleId = Convert.ToInt32(vehicle.ns2classNo),
         VehicleCode = vehicle.ns2classNo + "-" + vehicle.ns2typeNo,
         VehicleName = $"{vehicle.ns2typeFullname}-{vehicle.ns2transmission}-{GetFuelName(vehicle.ns2fuelType)}",
         FuelType = GetFuelType(vehicle.ns2fuelType),
         FuelTypeName = GetFuelName(vehicle.ns2fuelType),
         TransmissionType = GetTransmissionsType(vehicle.ns2transmission),
         TransmissionTypeName = vehicle.ns2transmission,
         SippCode = vehicle.ns2classCode,
         VendorMinimumDriverAge = ns2VehicleClass?.Ns2MinDriverAge?.ToIntNullSafe() ?? null,
         VendorMinimumDrivingLicenseAge = ns2VehicleClass?.Ns2MinDlicensePeriod?.ToIntNullSafe() ?? null
     }
     : null;
        public static string GetFuelName(string fuelType)
        {
            return fuelType switch
            {
                "O" => "Oil",
                "D" => "Diesel",
                "E" => "Electric",
                "EO" => "Electric+Oil",
                "ED" => "Electric+Diesel",
                "GO" => "LPG+Oil",
                _ => "Bilinmiyor"
            };
        }
        public static FuelTypes GetFuelType(string fuelType)
        {
            return fuelType switch
            {
                "O" => FuelTypes.Gasoline,
                "D" => FuelTypes.Diesel,
                "E" => FuelTypes.Electric,
                "EO" => FuelTypes.HybritGasoline,
                "ED" => FuelTypes.HybritDiesel,
                "GO" => FuelTypes.GasolineAndLPG,
                _ => FuelTypes.Undefined
            };
        }
        public static TransmissionTypes GetTransmissionsType(string fuelType)
        {
            return fuelType switch
            {
                "MAN" => TransmissionTypes.Manuel,
                "AUT" => TransmissionTypes.Automatic,
                "CVT" => TransmissionTypes.Automatic,
                "HAU" => TransmissionTypes.SemiAutomatic,
                _ => TransmissionTypes.Manuel
            };
        }
        public static Vehicle Map(this Ns2VehicleClasses vehicle, string vehicleTypeNo) =>
             vehicle != null ? new Vehicle
             {
                 VendorName = "Dailydrive",
                 //VehicleId = Convert.ToInt32(vehicle.Ns2ClassNo),
                 VehicleId = Convert.ToInt32(vehicle.Ns2ClassNo),
                 VehicleCode = vehicle.Ns2ClassNo + "-" + vehicleTypeNo,
                 VehicleName = $"{vehicle.Ns2ClassName}-{vehicleTypeNo}",
                 //FuelType = vehicle.,
                 //FuelTypeName = vehicle.Fuel,
                 //TransmissionType = GetAkkorTransmissionType(vehicle.Transmission),
                 //TransmissionTypeName = vehicle.Transmission,
                 SippCode = vehicle.Ns2ClassCode,
                 //DepositPrice = vehicle.Provision.ToFloatNullSafe(),
                 VendorMinimumDriverAge = vehicle.Ns2MinDriverAge.ToIntNullSafe(),
                 VendorMinimumDrivingLicenseAge = vehicle.Ns2MinDlicensePeriod.ToIntNullSafe()
             }
             : null;



        public static List<Vehicle> Map(this List<Ns2VehicleClasses> vehicles)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                {
                    foreach (var vehicleType in vehicle.VehicleTypeNos)
                    {
                        _vehicles.Add(vehicle.Map(vehicleType));
                    }

                }
            return _vehicles;
        }

        public static List<Vehicle> Map(this List<Ns2CapacitiesClasses> capacities, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (capacities != null && capacities.Count != 0)
            {
                foreach (var capacity in capacities)
                {
                    if (capacity.Ns2VehicleTypes.Count > 0)
                    {
                        //foreach (var vehicle in capacity.Ns2VehicleTypes)
                        //{
                        //    if (vehicle.Ns2TypeNo != -1)
                        //    {
                        //        _vehicles.Add(vehicle.Map(additionalInformation, capacity));
                        //    }
                        //}

                        // var vehicle = capacity.Ns2VehicleTypes[0];
                        foreach (var vehicle in capacity.Ns2VehicleTypes)
                        {
                            if (vehicle.Ns2TypeNo != -1)
                            {
                                _vehicles.Add(vehicle.Map(additionalInformation, capacity, vendor));
                            }
                        }

                    }
                }
            }

            return _vehicles;
        }

        public static Vehicle Map(this Ns2VehicleTypes vehicle, ResponseReservationStepsAdditionalInformation additionalInformation, Ns2CapacitiesClasses capacities, Vendor vendor) =>
            capacities.Ns2VehicleTypes != null && capacities.Ns2VehicleTypes.Count > 0 ? new Vehicle
            {
                VendorName = additionalInformation.Vendor.VendorName,
                VehicleId = Convert.ToInt32(vehicle.Ns2TypeNo),
                VehicleCode = $"{capacities.Ns2ClassNo.ToString()}-{vehicle.Ns2TypeNo}",
                VehicleName = $"{vehicle.Ns2TypeFullname}",
                VendorId = additionalInformation.Vendor.VendorId,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                FuelType = GetDailydriveFuelType(capacities.ns2ClassName),
                FuelTypeName = GetDailydriveFuelType(capacities.ns2ClassName).ToString(),
                TransmissionType = GetDailydriveTransmissionType(vehicle.Ns2Transmission),
                TransmissionTypeName = GetDailydriveTransmissionType(vehicle.Ns2Transmission).ToString(),
                SippCode = vehicle.Ns2TypeNo.ToString() + "-" + capacities.Ns2CampaignNo.ToString() + "-" + capacities.Ns2ClassCode,
                DepositPrice = capacities.Ns2Provision.ToFloatNullSafe(), // Depozito gelmemekte
                VendorMinimumDriverAge = capacities.Ns2MinDriverAge.ToIntNullSafe(),
                VendorMinimumDrivingLicenseAge = capacities.Ns2MinDlicencePeriod.ToIntNullSafe(),
                IsAvailable = capacities.Ns2Available.ToBoolNullSafe() && capacities.Ns2PickupInsideOfficeHours.ToBoolNullSafe() && capacities.Ns2ReturnInsideOfficeHours.ToBoolNullSafe(),
                DailyPrice = (float)capacities.Ns2Tariffs[0].Ns2TotalRentalPrice / (float)capacities.Ns2RentalDays,
                TotalPrice = (float)capacities.Ns2Tariffs[0].Ns2TotalRentalPrice,
                TotalPricePayNow = (float)capacities.Ns2Tariffs[0].Ns2TotalRentalPrice,
                PickupDateTime = capacities.Ns2PickupDate.ToDateTimeNullSafe(),
                ReturnDateTime = capacities.Ns2ReturnDate.ToDateTimeNullSafe(),
                DailyPricePayNow = (float)capacities.Ns2Tariffs[0].Ns2TotalRentalPrice / (float)capacities.Ns2RentalDays,
                RentalDuration = capacities.Ns2RentalDays,
                DailyKMLimit = capacities.Ns2KmLimit,
                TotalKMLimit = capacities.Ns2KmLimitType == "D" ? capacities.Ns2KmLimit * capacities.Ns2RentalDays : capacities.Ns2KmLimit,
                VendorLogo = additionalInformation.Vendor.Logo,
                OneWayFee = capacities.Ns2OnewayPrice.ToFloatNullSafe(),
                VehicleImages = new List<VehicleImage>
                  {
                        new VehicleImage
                        {
                            Url = string.IsNullOrEmpty(vehicle.Img1) ? "/design/img/no-image.png" : "https://jupicar.cloud:2028/image/" + vehicle.Img1
                        }
                  },
                VehicleType = GetDilydriveVehicleType(capacities.Ns2VehicleTypes[0].Ns2Body),
                VehicleCategoryType = VehicleCategoryTypes.None,
                VehicleCategoryTypeName = string.Empty,
                PassangerQuantityType = PassangerQuantityTypes.None,
                BaggageQuantityType = BaggageQuantityTypes.None,
                IsThereAirCondition = true,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
            } : null;

        private static FuelTypes GetDailydriveFuelType(string className)
        {
            return className.Contains("Benzin") ? FuelTypes.Gasoline : FuelTypes.Diesel;
        }
        private static TransmissionTypes GetDailydriveTransmissionType(string transmissionType)
        {
            return transmissionType == "AUT" ? TransmissionTypes.Automatic : transmissionType == "MAN" ? TransmissionTypes.Manuel : TransmissionTypes.SemiAutomatic;
        }
        private static VehicleTypes GetDilydriveVehicleType(string vehicleType)
        {
            switch (vehicleType)
            {
                case "HBA": return VehicleTypes.FiveDoorHatchback;
                case "SUV": return VehicleTypes.SUV;
                case "SED": return VehicleTypes.Sedan;
                case "WVN": return VehicleTypes.Van;
                default: return VehicleTypes.None;
            }
        }
    }
}
