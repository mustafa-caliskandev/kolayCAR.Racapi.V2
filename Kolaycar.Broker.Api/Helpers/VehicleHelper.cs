using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Helpers
{
    public class VehicleHelper
    {
        public static CommonModels.Vehicle MapVehicleProp(
            List<Vehiclecategorylang> vehicleCategories,
            List<Vehiclefuellang> vehicleFuels,
            List<Vehicletransmissionlang> vehicleTransmissions,
            List<Vehicletypelang> vehicletypes,
            CommonModels.Vehicle vehicle,
            CommonModels.LanguageTypes languageType,
            CommonModels.Vendor vendor = null)
        {
            var category = vehicleCategories.Where(x => x.Categoryid == (int)vehicle.VehicleCategoryType + 1 && x.Langid == (int)languageType + 1).FirstOrDefault();
            var fuel = vehicleFuels.Where(x => x.Fuelid == (int)vehicle.FuelType + 1 && x.Langid == (int)languageType + 1).FirstOrDefault();
            var transmission = vehicleTransmissions.Where(x => x.Transmissionid == (int)vehicle.TransmissionType + 1 && x.Langid == (int)languageType + 1).FirstOrDefault();
            var type = vehicletypes.Where(x => x.Typeid == (int)vehicle.VehicleType + 1 && x.Langid == (int)languageType + 1).FirstOrDefault();

            vehicle.VehicleCategoryTypeName = category?.Categoryname;
            vehicle.FuelTypeName = fuel?.Fuelname;
            vehicle.TransmissionTypeName = transmission?.Transmissionname;
            vehicle.VehicleTypeName = type?.Typename;
            if (vendor != null) vehicle.FindeksRequired = vendor.FindeksRequired;

            return vehicle;
        }

        public static CommonModels.Vehicle MapLocalVehicle(CommonModels.Vehicle vehicle, CommonModels.Vehicle localVehicle, List<ExchangeRates> exchangeRates = null, bool useBaseVehiclePropsFromVendorAPI = false, CurrencyTypes sourceCurrenyType = CurrencyTypes.TRY, CurrencyTypes targetCurrenyType = CurrencyTypes.TRY, bool useLocalDeposit = false, CommonModels.Vendor vendor = null)
        {
            vehicle.VehicleId = localVehicle.VehicleId;
            vehicle.VehicleCode = localVehicle.VehicleCode;

            if (!useBaseVehiclePropsFromVendorAPI)
            {
                if (localVehicle.DepositPrice != 0 && localVehicle.DepositPrice != null && useLocalDeposit)
                {
                    vehicle.DepositPrice = localVehicle.DepositPrice;
                    if (vendor != null)
                        sourceCurrenyType = vendor.CurrencyType;
                    if (sourceCurrenyType != targetCurrenyType)
                    {
                        if (exchangeRates != null)
                        {
                            var sourceExchangeRate = exchangeRates.Where(x => x.CurrencyType == sourceCurrenyType).FirstOrDefault();
                            var targetExchangeRate = exchangeRates.Where(x => x.CurrencyType == targetCurrenyType).FirstOrDefault();

                            vehicle.DepositPrice = CalculationHelper.RoundPrice((float)vehicle.DepositPrice * sourceExchangeRate.ExchangeRate / targetExchangeRate.ExchangeRate, 2);
                        }
                    }
                }

                vehicle.VendorMinimumDriverAge = localVehicle.VendorMinimumDriverAge ?? vehicle.VendorMinimumDriverAge;
                vehicle.VendorMinimumDrivingLicenseAge = localVehicle.VendorMinimumDrivingLicenseAge ?? vehicle.VendorMinimumDrivingLicenseAge;

                //if (localVehicle.DailyKMLimit.HasValue && localVehicle.DailyKMLimit != 0 && localVehicle.TotalKMLimit.HasValue && localVehicle.TotalKMLimit != 0)
                if (localVehicle.DailyKMLimit.HasValue && localVehicle.TotalKMLimit.HasValue)
                    vehicle.TotalKMLimit = localVehicle.DailyKMLimit * vehicle.RentalDuration > localVehicle.TotalKMLimit ? localVehicle.TotalKMLimit : localVehicle.DailyKMLimit * vehicle.RentalDuration;
                //else if (localVehicle.DailyKMLimit.HasValue && localVehicle.DailyKMLimit != 0 && (!localVehicle.TotalKMLimit.HasValue || localVehicle.TotalKMLimit == 0))
                else if (localVehicle.DailyKMLimit.HasValue && (!localVehicle.TotalKMLimit.HasValue))
                    vehicle.TotalKMLimit = localVehicle.DailyKMLimit * vehicle.RentalDuration;
                //else if (localVehicle.TotalKMLimit.HasValue && localVehicle.TotalKMLimit != 0 && (!localVehicle.DailyKMLimit.HasValue || localVehicle.DailyKMLimit == 0))
                else if (localVehicle.TotalKMLimit.HasValue && (!localVehicle.DailyKMLimit.HasValue))
                    vehicle.TotalKMLimit = localVehicle.TotalKMLimit.Value;

                // if (localVehicle.DailyKMLimit.HasValue && localVehicle.DailyKMLimit != 0)
                if (localVehicle.DailyKMLimit.HasValue)
                    vehicle.DailyKMLimit = localVehicle.DailyKMLimit;
            }

            vehicle.VehicleName = localVehicle.VehicleName;
            vehicle.SippCode = localVehicle.SippCode;
            vehicle.VehicleCategoryType = localVehicle.VehicleCategoryType;
            vehicle.VehicleCategoryTypeName = localVehicle.VehicleCategoryTypeName;
            vehicle.VehicleType = localVehicle.VehicleType;
            vehicle.VehicleTypeName = localVehicle.VehicleTypeName;
            vehicle.PassangerQuantityType = localVehicle.PassangerQuantityType;
            vehicle.PassangerQuantityName = localVehicle.PassangerQuantityName;
            vehicle.BaggageQuantityType = localVehicle.BaggageQuantityType;
            vehicle.BaggageQuantityName = localVehicle.BaggageQuantityName;
            vehicle.TransmissionType = localVehicle.TransmissionType;
            vehicle.TransmissionTypeName = localVehicle.TransmissionTypeName;
            vehicle.FuelType = localVehicle.FuelType;
            vehicle.FuelTypeName = localVehicle.FuelTypeName;
            vehicle.IsThereAirCondition = localVehicle.IsThereAirCondition;
            vehicle.VehicleImages = localVehicle.VehicleImages;
            vehicle.VehicleModelName = localVehicle.VehicleModelName;
            vehicle.VehicleBrandName = localVehicle.VehicleBrandName;

            vehicle.VehicleDescription = localVehicle.VehicleDescription;
            vehicle.VehicleClassNo = localVehicle.VehicleClassNo;
            vehicle.VehicleGroupName = localVehicle.VehicleGroupName;

            vehicle.OldDriverMinAge = localVehicle.OldDriverMinAge;
            vehicle.OldDriverMaxAge = localVehicle.OldDriverMaxAge;
            vehicle.YoungDriverMinAge = localVehicle.YoungDriverMinAge;
            vehicle.YoungDriverMaxAge = localVehicle.YoungDriverMaxAge;

            return vehicle;
        }

        public static List<CommonModels.Vehicle> MapLocalVehicleList(List<CommonModels.Vehicle> vehicles, List<CommonModels.Vehicle> localVehicles, List<ExchangeRates> exchangeRates = null, CurrencyTypes sourceCurrenyType = CurrencyTypes.TRY, CurrencyTypes targetCurrenyType = CurrencyTypes.TRY, bool useBaseVehiclePropsFromVendorAPI = false, bool useLocalDeposit = false, Domain.Models.Vendor vendor = null)
        {
            var newVehicleList = new List<CommonModels.Vehicle>();
            for (int i = 0; i < vehicles.Count; i++)
                for (int j = 0; j < localVehicles.Count; j++)
                    if (vehicles[i].VehicleCode == localVehicles[j].VehicleCode)
                    {
                        var apiVehicle = JsonConvert.DeserializeObject<CommonModels.Vehicle>(JsonConvert.SerializeObject(vehicles[i]));
                        var localVehicle = JsonConvert.DeserializeObject<CommonModels.Vehicle>(JsonConvert.SerializeObject(localVehicles[j]));

                        newVehicleList.Add(MapLocalVehicle(apiVehicle, localVehicle, exchangeRates, useBaseVehiclePropsFromVendorAPI,
                            sourceCurrenyType, targetCurrenyType, useLocalDeposit: useLocalDeposit, vendor: vendor));
                    }

            return newVehicleList;
        }

        public static float ConvertCommaFreePriceToFloat(string price)
        {
            try
            {
                if (!string.IsNullOrEmpty(price))
                {
                    if (price.Length > 0)
                    {
                        if (price.Length == 1)
                            price = $"0{price}";
                        var a = float.TryParse(price.Insert(price.Length - 2, ","), out float parsedPrice23) ? parsedPrice23 : 0;
                        return float.TryParse(price.Insert(price.Length - 2, ","), out float parsedPrice) ? parsedPrice : 0;
                    }
                }

                return 0;
            }
            catch (System.Exception)
            {
                //TODO: Logging
                return 0;
            }
        }

        public static void SetVehiclePropertyBySIPPCode(Vehicle vehicle)
        {
            if (vehicle != null && !string.IsNullOrEmpty(vehicle.SippCode) && vehicle.SippCode.Length == 4)
            {
                if (vehicle.VehicleCategoryType == VehicleCategoryTypes.None)
                    vehicle.VehicleCategoryType = GetVehicleCategory(vehicle.SippCode[0].ToString());

                if (vehicle.VehicleType == VehicleTypes.None)
                    vehicle.VehicleType = GetVehicleType(vehicle.SippCode[1].ToString());

                if (vehicle.TransmissionType == TransmissionTypes.None)
                    vehicle.TransmissionType = GetTransmissionTypes(vehicle.SippCode[2].ToString());

                if (vehicle.FuelType == FuelTypes.None)
                    vehicle.FuelType = GetFuelTypes(vehicle.SippCode[3].ToString());

            }

        }

        private static VehicleCategoryTypes GetVehicleCategory(string sippVehicleCategory)
        {
            switch (sippVehicleCategory.ToUpper())
            {
                default: return VehicleCategoryTypes.None;
                case "C":
                case "D":
                    return VehicleCategoryTypes.Compact;
                case "E":
                case "H":
                    return VehicleCategoryTypes.Economic;
                case "F":
                case "G":
                    return VehicleCategoryTypes.FullSize;
                case "L":
                case "W":
                    return VehicleCategoryTypes.Luxury;
                case "I":
                case "J":
                    return VehicleCategoryTypes.Intermediate;
                case "M":
                    return VehicleCategoryTypes.Minivan;
                case "N":
                    return VehicleCategoryTypes.Minibus;
                case "S":
                case "R":
                    return VehicleCategoryTypes.Standard;
            }
        }

        private static VehicleTypes GetVehicleType(string sippVehicleType)
        {
            switch (sippVehicleType.ToUpper())
            {
                default: return VehicleTypes.None;
                case "B":
                    return VehicleTypes.ThreeDoorHatchback;
                case "C":
                    return VehicleTypes.FiveDoorHatchback;
                case "D":
                    return VehicleTypes.Sedan;
                case "E":
                    return VehicleTypes.Coupe;
                case "F":
                    return VehicleTypes.SUV;
                case "P":
                case "Q":
                    return VehicleTypes.Pickup;
                case "V":
                    return VehicleTypes.Van;
                case "T":
                    return VehicleTypes.Cabrio;
            }
        }

        private static TransmissionTypes GetTransmissionTypes(string sippTransmissionTypes)
        {
            switch (sippTransmissionTypes.ToUpper())
            {
                default: return TransmissionTypes.None;
                case "M":
                case "N":
                case "C":
                    return TransmissionTypes.Manuel;
                case "A":
                case "B":
                case "D":
                    return TransmissionTypes.Automatic;
            }
        }

        private static FuelTypes GetFuelTypes(string sippFuelType)
        {
            switch (sippFuelType.ToUpper())
            {
                default: return FuelTypes.None;
                case "D":
                case "Q":
                    return FuelTypes.Diesel;
                case "E":
                case "C":
                    return FuelTypes.Electric;
                case "H":
                case "I":
                    return FuelTypes.HybritDiesel;
                case "L":
                case "S":
                case "M":
                case "F":
                    return FuelTypes.GasolineAndLPG;
                case "R":
                case "V":
                case "Z":
                    return FuelTypes.Gasoline;
            }
        }

        public static void SetVehiclesProperties(List<Vehicle> vehicleList, CommonModels.Vendor vendor, CommonModels.Agency agency, List<ExchangeRates> exchangeRates, CurrencyTypes baseVendorRequestCurrencyType, CurrencyTypes requestCurrencyType, List<CommonModels.ProfitMarkup> profitMarkups = null)
        {
            vehicleList.ForEach(x =>
            {
                x.DepositPrice = (vendor.VendorType == VendorTypes.GreenMotion || vendor.VendorType == VendorTypes.Yolcu360) && !vendor.DisableDeposit ? x.DepositPrice : vendor.DisableDeposit ? 0 : x.DepositPrice;
                //x.DepositPrice = (vendor.VendorType == VendorTypes.GreenMotion || vendor.VendorType == VendorTypes.Yolcu360) && !vendor.DisableDeposit ? x.DepositPrice : vendor.DisableDeposit ? 0 : x.DepositPrice != null ? CalculationHelper.CurrencyExchange(exchangeRates, vendor, x.DepositPrice.ToFloatNullSafe(), //vendor.VehicleMappingActive ? vendor.CurrencyType : baseVendorRequestCurrencyType, requestCurrencyType) : x.DepositPrice;
                //    vendor.VehicleMappingActive ? baseVendorRequestCurrencyType : baseVendorRequestCurrencyType, requestCurrencyType) : x.DepositPrice;
                //// vendor.VehicleMappingActive ? CurrencyTypes.TRY : baseVendorRequestCurrencyType, requestCurrencyType) : x.DepositPrice;
                x.ServiceCharge = CalculationHelper.CurrencyExchange(exchangeRates, vendor, vendor.ServiceCharge, vendor.ServiceChargeCurrencyType, requestCurrencyType);
                x.DepositCreditCardRequired = (vendor.VendorType == VendorTypes.Yolcu360 || vendor.VendorType == VendorTypes.Yolcu360v2) ? x.DepositCreditCardRequired : vendor.DepositCreditCardRequired;
                x.PersonalNumberRequired = vendor.PersonelNumberRequired;
                x.BaseVendorCurrencyTypes = baseVendorRequestCurrencyType;
                x = CalculationHelper.CalculateFinalVehiclePrices(vendor, x, agency, requestCurrencyType, profitMarkups, exchangeRates);
            });
        }

        public static List<T> SelectCheapestByGroup<T, TGroupKey>(
                List<T> items,
                Func<T, TGroupKey> groupKeySelector,
                Func<T, float> priceSelector)
        {
            var result = new List<T>();
            foreach (var group in items.GroupBy(groupKeySelector))
            {
                if (group.Count() == 1)
                    result.AddRange(group);
                else
                {
                    var cheapest = group.OrderBy(priceSelector).First();
                    result.Add(cheapest);
                }
            }
            return result;
        }
    }
}
