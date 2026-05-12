using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Mappers.AutoHome
{
    public static class VehicleMapper
    {

        public static Vehicle Map(this AutoHomeResponseBase.Vehicle vehicle,
            ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            int totalKmLimit = vehicle.TotalKmLimit.ToIntNullSafe();
            int days = vehicle.Days.ToIntNullSafe();
            int vendorDailyKmLimit = vehicle.DailyKmLimit.ToIntNullSafe();

            int dailyKm = days > 0 ? totalKmLimit / days : 0;

            int calculatedDailyKm = vendorDailyKmLimit > 0
                ? Math.Min(dailyKm, vendorDailyKmLimit)
                : dailyKm;

            int totalKm = calculatedDailyKm * days;

            return new Vehicle
            {
                VehicleId = vehicle.SubGroupId,
                VehicleCode = vehicle.SubGroupShortName,
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = vehicle.SubGroupShortName,
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = vehicle.SubGroupShortName,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = vehicle.Days.ToIntNullSafe(),
                DailyPrice = vehicle.DiscountedDailyPrice.ToFloatNullSafe(),
                OneWayFee = vehicle.DropPrice.ToFloatNullSafe(),
                TotalPrice = vehicle.GrandTotal.ToFloatNullSafe(),
                IsAvailable = true,
                VehicleType = VehicleTypes.None,
                TransmissionType = TransmissionTypes.None,
                VehicleCategoryType = VehicleCategoryTypes.None,
                PassangerQuantityType = PassangerQuantityTypes.None,
                FuelType = FuelTypes.Diesel,
                BaggageQuantityType = BaggageQuantityTypes.None,
                IsThereAirCondition = true,
                VendorMinimumDriverAge = vehicle.DriverMinAge,
                VendorMinimumDrivingLicenseAge = vehicle.DriverMinLicenceYear,
                DepositPrice = vehicle.ProvisionAmountTRY.ToFloatNullSafe(),
                TotalKMLimit = totalKm,
                DailyPricePayNow = vehicle.DiscountedDailyPrice.ToFloatNullSafe(),
                TotalPricePayNow = vehicle.GrandTotal.ToFloatNullSafe(),
                DailyKMLimit = calculatedDailyKm,
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                // Extras = CircularResponseBase.packages.Map()
                Extras = GetExtras(vehicle.Extras.Where(e => e.WebOnlineSelling == "1").ToList()),
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
            };
        }

        private static List<Extra> GetExtras(List<AutoHomeResponseBase.Extra> extras)
        {
            var extraList = new List<Extra>();
            if (extras != null)
            {
                foreach (var extra in extras)
                {
                    extraList.Add(new Extra
                    {
                        ExtraCode = extra.ProductCode,
                        ExtraName = extra.ProductName,
                        Price = extra.Amount.ToFloatNullSafe(),
                        ExtraDescription = extra.Description,
                        ExtraRentalType = ExtraRentalTypes.Daily
                    });
                }
            }
            return extraList;
        }

        public static Vehicle Map(this AutoHomeResponseBase.Vehicle vehicle, Vendor vendor)
        {
            if (vehicle != null)
            {
                return new Vehicle
                {
                    VendorName = vendor.VendorName,
                    VehicleId = vehicle.SubGroupId,
                    VehicleCode = vehicle.SubGroupShortName,
                    VehicleName = $"{vehicle.SubGroupName}",
                    FuelTypeName = vehicle.FuelType,
                    TransmissionTypeName = vehicle.Transmission,
                    VendorMinimumDriverAge = vehicle.YoungDriverMinAge.ToIntNullSafe(),
                    DepositPrice = vehicle.ProvisionAmountTRY.ToFloatNullSafe(),
                    IsAvailable = true,
                };
            }
            else
            {
                return null;
            }
        }


        public static List<Vehicle> Map(this List<AutoHomeResponseBase.Vehicle> data, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            foreach (var item in data)
                _vehicles.Add(item.Map(vendor));

            return _vehicles;
        }

        public static List<Vehicle> Map(this List<AutoHomeResponseBase.Vehicle> data, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            foreach (var item in data)
                _vehicles.Add(item.Map(additionalInformation, vendor));

            return _vehicles;
        }
    }
}