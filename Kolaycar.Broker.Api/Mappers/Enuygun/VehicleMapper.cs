using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;


namespace KolayCAR.Broker.API.Mappers.Enuygun
{
    public static class VehicleMapper
    {
        public static List<Vehicle> Map(this List<EnuygunResponse.Search.Reservation> apiVehicles, ResponseReservationStepsAdditionalInformation additionalInformation, CurrencyTypes currency, Vendor vendor, string requestID, List<ExchangeRates> exchangeRates)
        {
            var _vehicles = new List<Vehicle>();

            foreach (var item in apiVehicles)
            {
                var mappedVehicle = item.Map(additionalInformation, currency, vendor, requestID, exchangeRates);
                if (mappedVehicle != null)
                    _vehicles.Add(mappedVehicle);
            }

            return _vehicles;
        }

        public static Vehicle Map(this EnuygunResponse.Search.Reservation apiVehicle, ResponseReservationStepsAdditionalInformation additionalInformation, CurrencyTypes currency, Vendor vendor, string requestID, List<ExchangeRates> exchangeRates)
        {
            if (apiVehicle?.vehicle == null)
                return null;

            var reservationBreakdown = apiVehicle.breakdowns?.FirstOrDefault(e => e?.type == "reservation");
            var dropPrice = apiVehicle.breakdowns?.FirstOrDefault(e => e?.type == "drop");

            var totalPrice = apiVehicle.price.totalPrice;

            var vehicleDailyPrice = Math.Round(reservationBreakdown.totalPrice / apiVehicle.days, 2).ToFloatNullSafe();

            float depositPrice = 0;

            if (apiVehicle.provisionPrice != null && !string.IsNullOrWhiteSpace(apiVehicle.provisionPrice.currency) &&
                Enum.TryParse(apiVehicle.provisionPrice.currency, true, out CurrencyTypes depositCurrency))
            {
                depositPrice = CalculationHelper.CurrencyExchange(exchangeRates, vendor, apiVehicle.provisionPrice.price.ToLongNullSafe(), depositCurrency, currency);
            }

            var onewayFee = dropPrice?.totalPrice ?? 0;

            var companyName = apiVehicle.company?.name ?? string.Empty;
            var companyLogo = apiVehicle.company?.logoUri ?? string.Empty;
            var vehicleImageUrl = apiVehicle.vehicle.imageUrl ?? string.Empty;

            string vendorName = vendor.ShowSubVendorLogo == true && !string.IsNullOrWhiteSpace(companyName) ? companyName : vendor.VendorName;
            string vendorLogo = vendor.ShowSubVendorLogo == true && !string.IsNullOrWhiteSpace(companyLogo) ? companyLogo : vendor.Logo;

            var vehicle = new Vehicle
            {
                VehicleId = 0,
                VehicleCode = apiVehicle.vehicle.matchCode,
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = vendorName,
                ApiVendorName = apiVehicle.company.name,
                VendorLogo = vendorLogo,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VehicleName = (apiVehicle.vehicle.brand + " " + apiVehicle.vehicle.name).Trim(),
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = apiVehicle.days,
                OneWayFee = onewayFee,
                TotalPrice = apiVehicle.price.totalPrice,
                DailyPrice = vehicleDailyPrice,
                IsAvailable = true,
                VehicleImages = new() { new() { Url = apiVehicle.vehicle.imageUrl } },
                VehicleCategoryType = GetEnUygunVehicleCategoryType(apiVehicle.vehicle.@class),
                PassangerQuantityType = (PassangerQuantityTypes)apiVehicle.vehicle.chair,
                PassangerQuantityName = apiVehicle.vehicle.chair.ToStringNullSafe() + " Kişi",
                FuelType = GetEnUygunFuelType(apiVehicle.vehicle.fuel),
                TransmissionType = GetEnUygunTransmissionType(apiVehicle.vehicle.transmission),
                DeliveryType = GetEnUygunDeliveryType(apiVehicle.deliveryType),
                BaggageQuantityType = BaggageQuantityTypes.None,
                IsThereAirCondition = true,
                VendorMinimumDriverAge = apiVehicle.driverAge,
                VendorMinimumDrivingLicenseAge = apiVehicle.licenceYear,
                TotalPricePayNow = totalPrice,
                DepositPrice = depositPrice,
                TotalKMLimit = apiVehicle.limitedKm,
                DailyKMLimit = apiVehicle.dailyLimitedKm,
                FullCredit = (vendor.CreditType == CreditType.FullCredit) ? true : false,
                IsOffice = GetEnUygunDeliveryType(apiVehicle.deliveryType) == DeliveryType.FromOffice,
                VehicleType = VehicleTypes.None,
                VehicleBrandName = apiVehicle.vehicle.brand,
                VehicleModelName = apiVehicle.vehicle.name,
                VendorType = VendorTypes.EnUygun,
                ShortAddress = "Request Id = " + requestID + " Reservation ID = " + apiVehicle.referenceId,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice,
                PickupLocationAddress = apiVehicle.pickUpOffice?.address ?? "",
                ReturnLocationAddress = apiVehicle.pickUpOffice?.address ?? ""
            };
            return vehicle;
        }

        private static VehicleCategoryTypes GetEnUygunVehicleCategoryType(string vehicleCategoryType)
        {
            switch (vehicleCategoryType)
            {
                case "economic": return VehicleCategoryTypes.Economic;
                case "intermediate": return VehicleCategoryTypes.Intermediate;
                case "suv": return VehicleCategoryTypes.SUV;
                case "comfort": return VehicleCategoryTypes.FullSize;
                case "premium": return VehicleCategoryTypes.Premium;
                case "luxury": return VehicleCategoryTypes.Luxury;
                case "compact": return VehicleCategoryTypes.Compact;
                case "compactElite": return VehicleCategoryTypes.CompactElite;
                case "standard": return VehicleCategoryTypes.Standard;
                case "van": return VehicleCategoryTypes.Minivan;
                case "mediumSuv": return VehicleCategoryTypes.SUV4x2;
                case "prestige": return VehicleCategoryTypes.Prestige;
                default: return VehicleCategoryTypes.None;
            }
        }
        private static FuelTypes GetEnUygunFuelType(string fuelType)
        {
            switch (fuelType)
            {
                case "gas": return FuelTypes.Gasoline;
                case "diesel": return FuelTypes.Diesel;
                case "lpg": return FuelTypes.GasolineAndLPG;
                case "diesel-gas-mixed": return FuelTypes.HybritDiesel;
                case "hybrid": return FuelTypes.HybritGasoline;
                case "gas-diesel": return FuelTypes.GasolineAndDiesel;
                case "electric": return FuelTypes.Electric;
                default: return FuelTypes.None;
            }
        }
        private static TransmissionTypes GetEnUygunTransmissionType(string transmissionType)
        {
            switch (transmissionType)
            {
                case "manual": return TransmissionTypes.Manuel;
                case "automatic": return TransmissionTypes.Automatic;
                default: return TransmissionTypes.None;
            }
        }
        private static DeliveryType GetEnUygunDeliveryType(string type)
        {
            switch (type)
            {
                case "officeDelivery": return DeliveryType.FromOffice;
                case "airportMeetAndGreet": return DeliveryType.MeetAndGreet;
                default: return DeliveryType.None;
            }
        }
    }
}
