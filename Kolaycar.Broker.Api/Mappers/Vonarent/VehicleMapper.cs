using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Response.Vonarent;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Mappers.Vonarent
{
    public static class VehicleMapper
    {
        public static List<Vehicle> Map(this List<VonarentVehicleItem> apiVehicleList, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var vehicles = new List<Vehicle>();
            if (apiVehicleList == null)
                return vehicles;

            for (int i = 0; i < apiVehicleList.Count; i++)
            {
                var apiVehicle = apiVehicleList[i];
                vehicles.Add(new Vehicle
                {
                    VehicleId = i + 1,
                    VendorId = vendor.VendorId,
                    VendorName = vendor.VendorName,
                    ApiVendorName = vendor.VendorName,
                    VendorPhone = vendor.VendorPhone,
                    VendorEmail = vendor.VendorEmail,
                    VendorLogo = vendor.Logo,
                    VehicleCode = apiVehicle.id,
                    VehicleName = apiVehicle.name.ToStringNullSafe(),
                    VehicleDescription = BuildVehicleDescription(apiVehicle.properties),
                    DailyPrice = apiVehicle.priceDaily,
                    DailyPricePayNow = apiVehicle.priceDaily,
                    TotalPrice = apiVehicle.price,
                    TotalPricePayNow = apiVehicle.price,
                    OneWayFee = 0,
                    ExtraPrice = 0,
                    RentalDuration = additionalInformation.RentalDuration,
                    IsAvailable = true,
                    CurrencyCode = apiVehicle.currency.ToStringNullSafe(),
                    PassangerQuantityType = MapPassengerQuantity(apiVehicle.capacity),
                    PassangerQuantityName = apiVehicle.capacity > 0 ? apiVehicle.capacity.ToString() : string.Empty,
                    BaggageQuantityType = MapBaggageQuantity(apiVehicle.luggageVolume),
                    BaggageQuantityName = apiVehicle.luggageVolume > 0 ? apiVehicle.luggageVolume.ToString() : string.Empty,
                    FuelType = MapFuelType(apiVehicle.fuel),
                    FuelTypeName = apiVehicle.fuel.ToStringNullSafe(),
                    TransmissionType = MapTransmissionType(apiVehicle.properties),
                    TransmissionTypeName = GetTransmissionName(apiVehicle.properties),
                    VehicleCategoryType = MapVehicleCategory(apiVehicle.vehicleType),
                    VehicleCategoryTypeName = apiVehicle.vehicleType.ToStringNullSafe(),
                    VehicleType = MapVehicleType(apiVehicle.vehicleType, apiVehicle.properties),
                    VehicleTypeName = apiVehicle.vehicleType.ToStringNullSafe(),
                    IsThereAirCondition = HasAirCondition(apiVehicle.properties),
                    VehicleImages = string.IsNullOrWhiteSpace(apiVehicle.image)
                        ? new List<VehicleImage>()
                        : new List<VehicleImage> { new VehicleImage { Url = apiVehicle.image } },
                    PickupLocationId = additionalInformation.PickupLocationId,
                    PickupLocationCode = additionalInformation.APIPickupLocationCode,
                    PickupLocationName = additionalInformation.PickupLocationName,
                    ReturnLocationId = additionalInformation.ReturnLocationId,
                    ReturnLocationCode = additionalInformation.APIReturnLocationCode,
                    ReturnLocationName = additionalInformation.ReturnLocationName,
                    PickupDateTime = additionalInformation.PickupDateTime,
                    ReturnDateTime = additionalInformation.ReturnDateTime
                });
            }

            return vehicles;
        }

        public static CurrencyTypes GetCurrencyType(string currencyCode, CurrencyTypes fallbackCurrencyType)
            => Enum.TryParse(currencyCode, true, out CurrencyTypes currencyType) ? currencyType : fallbackCurrencyType;

        private static string BuildVehicleDescription(List<string> properties)
            => properties != null && properties.Count > 0 ? string.Join(" - ", properties.Where(x => !string.IsNullOrWhiteSpace(x))) : string.Empty;

        private static FuelTypes MapFuelType(string fuelType)
        {
            var normalized = fuelType.ToStringNullSafe().Trim().ToLowerInvariant();
            return normalized switch
            {
                "gasoline" or "petrol" or "benzin" => FuelTypes.Gasoline,
                "diesel" or "dizel" => FuelTypes.Diesel,
                "electric" or "elektrik" => FuelTypes.Electric,
                "hybrid" or "hibrit" => FuelTypes.Hybrid,
                "lpg" or "gasoline+lpg" or "benzin+lpg" => FuelTypes.GasolineAndLPG,
                _ => FuelTypes.None
            };
        }

        private static TransmissionTypes MapTransmissionType(List<string> properties)
        {
            var normalizedProperties = NormalizeProperties(properties);
            if (normalizedProperties.Any(x => x.Contains("semi automatic") || x.Contains("semi-automatic") || x.Contains("semi otomatik") || x.Contains("yari otomatik")))
                return TransmissionTypes.SemiAutomatic;

            if (normalizedProperties.Any(x => x.Contains("automatic") || x.Contains("otomatik")))
                return TransmissionTypes.Automatic;

            if (normalizedProperties.Any(x => x.Contains("manual") || x.Contains("manuel")))
                return TransmissionTypes.Manuel;

            return TransmissionTypes.None;
        }

        private static string GetTransmissionName(List<string> properties)
        {
            var transmissionType = MapTransmissionType(properties);
            return transmissionType switch
            {
                TransmissionTypes.Automatic => "Automatic",
                TransmissionTypes.Manuel => "Manual",
                TransmissionTypes.SemiAutomatic => "Semi Automatic",
                _ => string.Empty
            };
        }

        private static bool HasAirCondition(List<string> properties)
        {
            var normalizedProperties = NormalizeProperties(properties);
            return normalizedProperties.Any(x => x == "ac" || x.Contains("air condition") || x.Contains("klima"));
        }

        private static VehicleCategoryTypes MapVehicleCategory(string vehicleType)
        {
            var normalized = vehicleType.ToStringNullSafe().Trim().ToLowerInvariant();
            if (normalized.Contains("economic") || normalized.Contains("economy") || normalized.Contains("ekonom"))
                return VehicleCategoryTypes.Economic;
            if (normalized.Contains("compact") || normalized.Contains("kompakt"))
                return VehicleCategoryTypes.Compact;
            if (normalized.Contains("standard") || normalized.Contains("standart"))
                return VehicleCategoryTypes.Standard;
            if (normalized.Contains("intermediate") || normalized.Contains("orta"))
                return VehicleCategoryTypes.Intermediate;
            if (normalized.Contains("full") || normalized.Contains("large"))
                return VehicleCategoryTypes.FullSize;
            if (normalized.Contains("premium"))
                return VehicleCategoryTypes.Premium;
            if (normalized.Contains("luxury") || normalized.Contains("lux"))
                return VehicleCategoryTypes.Luxury;
            if (normalized.Contains("minivan"))
                return VehicleCategoryTypes.Minivan;
            if (normalized.Contains("minibus"))
                return VehicleCategoryTypes.Minibus;
            if (normalized.Contains("suv"))
                return VehicleCategoryTypes.SUV;
            return VehicleCategoryTypes.None;
        }

        private static VehicleTypes MapVehicleType(string vehicleType, List<string> properties)
        {
            var normalized = vehicleType.ToStringNullSafe().Trim().ToLowerInvariant();
            if (normalized.Contains("suv"))
                return VehicleTypes.SUV;
            if (normalized.Contains("sedan"))
                return VehicleTypes.Sedan;
            if (normalized.Contains("hatchback") || normalized.Contains("hb"))
                return VehicleTypes.FiveDoorHatchback;
            if (normalized.Contains("station") || normalized.Contains("wagon") || normalized.Contains("sw"))
                return VehicleTypes.StationWagon;
            if (normalized.Contains("van") || normalized.Contains("minivan"))
                return VehicleTypes.Van;
            if (normalized.Contains("pickup"))
                return VehicleTypes.Pickup;
            if (normalized.Contains("coupe"))
                return VehicleTypes.Coupe;
            if (normalized.Contains("cabrio") || normalized.Contains("convertible"))
                return VehicleTypes.Cabrio;

            var normalizedProperties = NormalizeProperties(properties);
            if (normalizedProperties.Any(x => x.Contains("suv")))
                return VehicleTypes.SUV;

            return VehicleTypes.None;
        }

        private static PassangerQuantityTypes MapPassengerQuantity(int capacity)
            => capacity switch
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
                10 => PassangerQuantityTypes.TenPerson,
                _ => PassangerQuantityTypes.None
            };

        private static BaggageQuantityTypes MapBaggageQuantity(int luggageVolume)
            => luggageVolume switch
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

        private static List<string> NormalizeProperties(List<string> properties)
            => properties?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToLowerInvariant()).ToList() ?? new List<string>();
    }
}
