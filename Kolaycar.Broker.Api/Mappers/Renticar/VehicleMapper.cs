using KolayCAR.Broker.API.Helpers.Renticar;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Renticar.Response;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Renticar
{
    public static class VehicleMapper
    {
        public static List<Vehicle> Map(this List<CarsResponseBase> apiVehicles)
        {
            var _vehicles = new List<Vehicle>();

            foreach (var item in apiVehicles)
            {
                _vehicles.Add(item.Map());
            }

            return _vehicles;
        }

        private static Vehicle Map(this CarsResponseBase apiVehicle)
        {
            return new Vehicle
            {
                VendorName = "Renticar",
                VehicleId = 0,
                VehicleCode = apiVehicle.carId.ToStringNullSafe(),
                VehicleName = apiVehicle.brand + " " + apiVehicle.model + " (" + GetTransmissionType(apiVehicle.transmission).ToString() + "-" + GetFuelType(apiVehicle.ful).ToString() + "-" + GetVehicleType(apiVehicle.body).ToString() + ")",
                FuelType = GetFuelType(apiVehicle.ful),
                TransmissionType = GetTransmissionType(apiVehicle.transmission),
                SippCode = apiVehicle.sipp.ToStringNullSafe()
            };
        }

        public static List<Vehicle> Map(this List<Offer> apiVehicles, ResponseReservationStepsAdditionalInformation additionalInformation, CurrencyTypes currency, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            foreach (var item in apiVehicles)
            {
                _vehicles.Add(item.Map(additionalInformation, currency, vendor, apiVehicles.IndexOf(item)));
            }

            return _vehicles;
        }

        private static Vehicle Map(this Offer apiVehicle, ResponseReservationStepsAdditionalInformation additionalInformation, CurrencyTypes currency, Vendor vendor, int index = 1)
        {
            var vehicle = new Vehicle
            {
                VehicleId = index.ToIntNullSafe(),
                VehicleCode = apiVehicle.car.carId,
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                ApiVendorName = apiVehicle.companySlug.ToStringNullSafe(),
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = $"{apiVehicle.car.brand} {apiVehicle.car.model}",
                VendorLogo = additionalInformation.Vendor.Logo,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = apiVehicle.totalDays.ToIntNullSafe(),
                DailyPrice = RenticarHelper.GetPrice(apiVehicle, currency, 1),
                OneWayFee = RenticarHelper.GetPrice(apiVehicle, currency, 3),
                TotalPrice = RenticarHelper.GetPrice(apiVehicle, currency, 2),
                IsAvailable = true,
                VehicleImages = new List<VehicleImage>(),
                VehicleType = GetVehicleType(apiVehicle.car.body),
                TransmissionType = GetTransmissionType(apiVehicle.car.transmission),
                VehicleCategoryType = GetVehicleCategoryType(apiVehicle.car.@class),
                PassangerQuantityType = (PassangerQuantityTypes)apiVehicle.car.seat.ToIntNullSafe(),
                FuelType = GetFuelType(apiVehicle.car.fuel),
                BaggageQuantityType = BaggageQuantityTypes.None,
                IsThereAirCondition = true,
                VendorMinimumDriverAge = apiVehicle.rules.driverAge.ToIntNullSafe(),
                VendorMinimumDrivingLicenseAge = apiVehicle.rules.licenseYears.ToIntNullSafe(),
                DailyPricePayNow = RenticarHelper.GetPrice(apiVehicle, currency, 1),
                TotalPricePayNow = RenticarHelper.GetPrice(apiVehicle, currency, 2),
                DepositPrice = RenticarHelper.GetPrice(apiVehicle, currency, 4),
                DailyKMLimit = apiVehicle.rules.dailyRangeLimit.ToIntNullSafe(),
                TotalKMLimit = apiVehicle.rules.totalRangeLimit.ToIntNullSafe(),
                FullCredit = (vendor.CreditType == CreditType.FullCredit && apiVehicle.isFullCreditAvailable.ToBoolNullSafe()) ? true : false,
                Extras = null,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
            };

            return vehicle;
        }

        private static FuelTypes GetFuelType(string fuelType) => fuelType == "gas" ? FuelTypes.Gasoline : FuelTypes.Diesel;

        private static TransmissionTypes GetTransmissionType(string transmissionType) => transmissionType == "automatic" ? TransmissionTypes.Automatic : TransmissionTypes.Manuel;

        private static VehicleTypes GetVehicleType(string vehicleTypes) => vehicleTypes == "sedan" ? VehicleTypes.Sedan : vehicleTypes == "hatchback" ? VehicleTypes.FiveDoorHatchback : vehicleTypes == "suv" || vehicleTypes == "crossover" ? VehicleTypes.SUV : vehicleTypes == "van" ? VehicleTypes.Van : vehicleTypes == "wagon" ? VehicleTypes.StationWagon : VehicleTypes.None;

        private static VehicleCategoryTypes GetVehicleCategoryType(string categoryType) => categoryType == "economic" ? VehicleCategoryTypes.Economic : categoryType == "medium" ? VehicleCategoryTypes.Standard : VehicleCategoryTypes.None;

        //private static float GetPrice(Offer apiVehicle, CurrencyTypes currency, int type)//type 1: daily price, 2: totalprice, 3: dropprice, 4: provission
        //{
        //    if (type == 1)
        //        return currency == CurrencyTypes.EUR ?
        //                apiVehicle.pricing.dailyPrice.EUR.ToFloatNullSafe() :
        //                currency == CurrencyTypes.USD ?
        //                    apiVehicle.pricing.dailyPrice.USD.ToFloatNullSafe() :
        //                    currency == CurrencyTypes.TRY ?
        //                        apiVehicle.pricing.dailyPrice.ToFloatNullSafe() : apiVehicle.pricing.dailyPrice.EUR.ToFloatNullSafe();
        //    if (type == 2)
        //        return currency == CurrencyTypes.EUR ?
        //                apiVehicle.pricing.totalPrice.EUR.ToFloatNullSafe() :
        //                currency == CurrencyTypes.USD ?
        //                    apiVehicle.pricing.totalPrice.USD.ToFloatNullSafe() :
        //                    currency == CurrencyTypes.TRY ?
        //                        apiVehicle.pricing.totalPrice.ToFloatNullSafe() : apiVehicle.pricing.totalPrice.EUR.ToFloatNullSafe();
        //    if (type == 3)
        //        return currency == CurrencyTypes.EUR ?
        //                apiVehicle.pricing.dropPrice.EUR.ToFloatNullSafe() :
        //                currency == CurrencyTypes.USD ?
        //                    apiVehicle.pricing.dropPrice.USD.ToFloatNullSafe() :
        //                    currency == CurrencyTypes.TRY ?
        //                        apiVehicle.pricing.dropPrice.ToFloatNullSafe() : apiVehicle.pricing.dropPrice.EUR.ToFloatNullSafe();
        //    if (type == 4)
        //        return currency == CurrencyTypes.EUR ?
        //                apiVehicle.pricing.provision.EUR.ToFloatNullSafe() :
        //                currency == CurrencyTypes.USD ?
        //                    apiVehicle.pricing.provision.USD.ToFloatNullSafe() :
        //                    currency == CurrencyTypes.TRY ?
        //                        apiVehicle.pricing.provision.ToFloatNullSafe() : apiVehicle.pricing.provision.EUR.ToFloatNullSafe();
        //    return 0;
        //}
    }
}
