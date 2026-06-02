using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Responses.YesOto;
using KolayCAR.Broker.Infrastructure.Extensions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Kolaycar.Broker.Api.Mappers.YesOto
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this YesOtoVehicleData vehicle, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            if (vehicle == null)
                return null;

            var vehicleCode = vehicle.vehicleGroup.id;
            var rentalDuration = GetRentalDuration(vehicle, additionalInformation);
            var oneWayFee = vehicle.discountedOneDirectionPrice > 0 ? vehicle.discountedOneDirectionPrice : vehicle.oneDirectionPrice;
            var totalPrice = vehicle.GetFinalTotal();
            var rentTotal = FirstPositive(vehicle.totalDiscountedPrice, vehicle.totalPrice, totalPrice - oneWayFee);
            var dailyPrice = FirstPositive(vehicle.discountedPricePerDay, vehicle.pricePerDay, rentTotal / rentalDuration);
            var vehicleName = FirstText(vehicle.name, vehicle.vehicleGroup?.text, vehicle.vehicleGroup?.name, vehicle.description);
            var gearTypeName = ToText(vehicle.gearType) ?? ToText(vehicle.vehicleGroup?.gearType);
            var fuelTypeName = ToText(vehicle.fuelType) ?? ToText(vehicle.vehicleGroup?.fuelType);
            var categoryName = ToText(vehicle.vehicleGroupClass) ?? ToText(vehicle.vehicleGroup?.vehicleGroupClass);
            var passengerCapacity = FirstPositiveInt(vehicle.adultCapacity, vehicle.vehicleGroup?.adultCapacity ?? 0);
            var baggageCapacity = FirstPositiveInt(vehicle.suiteCaseCapacityTotal, vehicle.vehicleGroup?.suiteCaseCapacityTotal ?? 0);
            var dailyKmLimit = vehicle.isDailyMilageLimit && vehicle.dailyKilometerLimit > 0 ? vehicle.dailyKilometerLimit : 0;

            return new Vehicle
            {
                VehicleId = ToStableId(vehicleCode),
                VehicleCode = vehicleCode,
                VendorId = vendor?.VendorId ?? additionalInformation?.Vendor?.VendorId ?? 0,
                VendorName = vendor?.VendorName ?? additionalInformation?.Vendor?.VendorName,
                VendorPhone = vendor?.VendorPhone ?? additionalInformation?.Vendor?.VendorPhone,
                VendorEmail = vendor?.VendorEmail ?? additionalInformation?.Vendor?.VendorEmail,
                VendorLogo = vendor?.Logo ?? additionalInformation?.Vendor?.Logo,
                ApiVendorName = vendor?.VendorName,
                VehicleName = vehicleName,
                VehicleDescription = vehicle.description,
                PickupLocationId = additionalInformation?.PickupLocationId ?? 0,
                PickupLocationName = additionalInformation?.PickupLocationName,
                PickupLocationCode = additionalInformation?.APIPickupLocationCode,
                ReturnLocationId = additionalInformation?.ReturnLocationId ?? 0,
                ReturnLocationName = additionalInformation?.ReturnLocationName,
                ReturnLocationCode = additionalInformation?.APIReturnLocationCode,
                PickupDateTime = additionalInformation?.PickupDateTime ?? default,
                ReturnDateTime = additionalInformation?.ReturnDateTime ?? default,
                RentalDuration = rentalDuration,
                DailyPrice = dailyPrice,
                OneWayFee = oneWayFee,
                TotalPrice = totalPrice,
                DailyPricePayNow = dailyPrice,
                TotalPricePayNow = totalPrice,
                DepositPrice = vehicle.blockedAmountForCreditCard,
                DepositCreditCardRequired = vehicle.blockedAmountForCreditCard > 0,
                IsAvailable = totalPrice > 0,
                VehicleImages = new List<VehicleImage> { new VehicleImage { Url = $"https://www.enterprise.com.tr/upload/vehicles/{vehicle.imageRight}" } },
                VehicleType = VehicleTypes.None,
                TransmissionType = MapTransmission(gearTypeName),
                TransmissionTypeName = gearTypeName,
                VehicleCategoryType = MapCategory(categoryName),
                VehicleCategoryTypeName = categoryName,
                PassangerQuantityType = MapPassanger(passengerCapacity),
                PassangerQuantityName = passengerCapacity > 0 ? passengerCapacity.ToString() : null,
                FuelType = MapFuel(fuelTypeName),
                FuelTypeName = fuelTypeName,
                BaggageQuantityType = MapBaggage(baggageCapacity),
                BaggageQuantityName = baggageCapacity > 0 ? baggageCapacity.ToString() : null,
                IsThereAirCondition = true,
                VendorMinimumDriverAge = vehicle.ageGroupMin > 0 ? vehicle.ageGroupMin : null,
                VendorMinimumDrivingLicenseAge = vehicle.drivingLicenseAge > 0 ? vehicle.drivingLicenseAge : null,
                DailyKMLimit = dailyKmLimit > 0 ? dailyKmLimit : null,
                TotalKMLimit = dailyKmLimit > 0 ? dailyKmLimit * rentalDuration : null,
                FullCredit = vendor?.CreditType == CreditType.FullCredit,
                RentalWorkingTypes = vendor?.RentalWorkingType ?? VendorWorkingTypes.ProfitMarkup,
                ProfitMarkupDailyPrice = vendor?.ProfitMarkupDailyPrice ?? 0,
                CurrencyCode = CurrencyTypes.TRY.ToString()
            };
        }

        public static List<Vehicle> Map(this List<YesOtoVehicleData> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            return vehicles?.Select(vehicle => vehicle.Map(additionalInformation, vendor)).Where(vehicle => vehicle != null).ToList() ?? new List<Vehicle>();
        }

        public static List<Vehicle> Map(this List<YesOtoVehicleData> vehicles)
        {
            return vehicles?.Select(vehicle => vehicle.Map(null, null)).Where(vehicle => vehicle != null).ToList() ?? new List<Vehicle>();
        }

        public static float GetFinalTotal(this YesOtoVehicleData vehicle)
        {
            if (vehicle == null)
                return 0;

            return FirstPositive(
                vehicle.discountedGrandTotal,
                vehicle.grandTotal,
                vehicle.totalDiscountedPrice + vehicle.discountedOneDirectionPrice,
                vehicle.totalPrice + vehicle.oneDirectionPrice,
                vehicle.totalDiscountedPrice,
                vehicle.totalPrice);
        }

        private static int GetRentalDuration(YesOtoVehicleData vehicle, ResponseReservationStepsAdditionalInformation additionalInformation)
        {
            if (vehicle.rentalDayCount > 0)
                return vehicle.rentalDayCount;

            if (additionalInformation != null && additionalInformation.RentalDuration > 0)
                return additionalInformation.RentalDuration;

            return 1;
        }

        private static string ToText(object value)
        {
            if (value == null)
                return null;

            if (value is string text)
                return FirstText(text);

            if (value is JValue jValue)
                return jValue.Value?.ToString();

            if (value is JObject jObject)
                return FirstText(
                    jObject.Value<string>("text"),
                    jObject.Value<string>("name"),
                    jObject.Value<string>("value"),
                    jObject.Value<string>("code"),
                    jObject.Value<string>("id"));

            var serializedValue = value.ToString();

            if (!string.IsNullOrWhiteSpace(serializedValue) && serializedValue.TrimStart().StartsWith("{"))
            {
                try
                {
                    var json = JObject.Parse(serializedValue);
                    return FirstText(
                        json.Value<string>("text"),
                        json.Value<string>("name"),
                        json.Value<string>("value"),
                        json.Value<string>("code"),
                        json.Value<string>("id"));
                }
                catch
                {
                    return serializedValue;
                }
            }

            return FirstText(serializedValue);
        }

        private static string FirstText(params string[] values)
        {
            return values?.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.TrimNullSafe();
        }

        private static float FirstPositive(params float[] values)
        {
            return values.FirstOrDefault(value => value > 0);
        }

        private static int FirstPositiveInt(params int[] values)
        {
            return values.FirstOrDefault(value => value > 0);
        }

        private static int ToStableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            unchecked
            {
                uint hash = 2166136261;

                foreach (var character in value)
                {
                    hash ^= character;
                    hash *= 16777619;
                }

                return (int)(hash & 0x7fffffff);
            }
        }

        private static TransmissionTypes MapTransmission(string gearType)
        {
            var normalized = gearType.ToStringNullSafe().ToLowerInvariant();

            if (normalized.Contains("auto") || normalized.Contains("otomatik"))
                return TransmissionTypes.Automatic;

            if (normalized.Contains("semi") || normalized.Contains("yari"))
                return TransmissionTypes.SemiAutomatic;

            if (normalized.Contains("manual") || normalized.Contains("manuel"))
                return TransmissionTypes.Manuel;

            return TransmissionTypes.None;
        }

        private static FuelTypes MapFuel(string fuelType)
        {
            var normalized = fuelType.ToStringNullSafe().ToLowerInvariant();

            if (normalized.Contains("diesel") || normalized.Contains("dizel"))
                return FuelTypes.Diesel;

            if (normalized.Contains("electric") || normalized.Contains("elektrik"))
                return FuelTypes.Electric;

            if (normalized.Contains("hybrid") || normalized.Contains("hibrit"))
                return FuelTypes.Hybrid;

            if (normalized.Contains("lpg"))
                return FuelTypes.GasolineAndLPG;

            if (normalized.Contains("gasoline") || normalized.Contains("petrol") || normalized.Contains("benzin"))
                return FuelTypes.Gasoline;

            return FuelTypes.None;
        }

        private static VehicleCategoryTypes MapCategory(string category)
        {
            var normalized = category.ToStringNullSafe().ToLowerInvariant();

            if (normalized.Contains("ekonomik") || normalized.Contains("economic"))
                return VehicleCategoryTypes.Economic;

            if (normalized.Contains("kompakt") || normalized.Contains("compact"))
                return VehicleCategoryTypes.Compact;

            if (normalized.Contains("standart") || normalized.Contains("standard"))
                return VehicleCategoryTypes.Standard;

            if (normalized.Contains("lux") || normalized.Contains("premium"))
                return VehicleCategoryTypes.Luxury;

            if (normalized.Contains("prestige"))
                return VehicleCategoryTypes.Prestige;

            if (normalized.Contains("suv"))
                return VehicleCategoryTypes.SUV;

            if (normalized.Contains("intermediate") || normalized.Contains("orta"))
                return VehicleCategoryTypes.Intermediate;

            if (normalized.Contains("full"))
                return VehicleCategoryTypes.FullSize;

            return VehicleCategoryTypes.None;
        }

        private static PassangerQuantityTypes MapPassanger(int capacity)
        {
            return capacity switch
            {
                1 => PassangerQuantityTypes.OnePerson,
                2 => PassangerQuantityTypes.TwoPerson,
                3 => PassangerQuantityTypes.ThreePerson,
                4 => PassangerQuantityTypes.FourPerson,
                5 => PassangerQuantityTypes.FivePerson,
                6 => PassangerQuantityTypes.SixPerson,
                7 => PassangerQuantityTypes.SevenPerson,
                8 => PassangerQuantityTypes.EightPerson,
                9 => PassangerQuantityTypes.NinePerson,
                _ => PassangerQuantityTypes.None
            };
        }

        private static BaggageQuantityTypes MapBaggage(int capacity)
        {
            return capacity switch
            {
                1 => BaggageQuantityTypes.One,
                2 => BaggageQuantityTypes.Two,
                3 => BaggageQuantityTypes.Three,
                4 => BaggageQuantityTypes.Four,
                5 => BaggageQuantityTypes.Five,
                6 => BaggageQuantityTypes.Six,
                7 => BaggageQuantityTypes.Seven,
                8 => BaggageQuantityTypes.Eight,
                _ => BaggageQuantityTypes.None
            };
        }
    }
}
