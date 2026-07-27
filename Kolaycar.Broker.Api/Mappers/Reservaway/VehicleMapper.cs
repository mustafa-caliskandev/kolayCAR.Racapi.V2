using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Response.Reservaway;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Mappers.Reservaway
{
    public static class VehicleMapper
    {
        public static List<Vehicle> Map(
            this List<ReservawayVehicleDetail> apiVehicleList,
            GetVehiclesRequest getVehiclesRequest,
            ResponseReservationStepsAdditionalInformation additionalInformation,
            Vendor vendor,
            CurrencyTypes requestCurrencyType,
            CurrencyTypes baseVendorRequestCurrencyType,
            List<ProfitMarkup> profitMarkups,
            List<ExchangeRates> exchangeRates,
            string visitorSessionId,
            List<ReservawayTypeItem> productTypes,
            List<ReservawayTypeItem> paymentTypes)
        {
            var vehicles = new List<Vehicle>();
            if (apiVehicleList == null)
                return vehicles;

            foreach (var apiVehicle in apiVehicleList)
            {
                var planDefinition = ReservawayMapperHelper.GetBasePlanDefinition(
                    apiVehicle.prices,
                    productTypes,
                    paymentTypes);

                if (planDefinition == null)
                    continue;

                var planReference = ReservawayMapperHelper.ParsePlanReference(ReservawayMapperHelper.CreatePlanReference(planDefinition));
                var vehicle = apiVehicle.Map(additionalInformation, vendor, planReference);
                if (vehicle == null)
                    continue;

                var apiRatePrice = GetSelectedRatePrice(apiVehicle, planReference);
                var apiDailyPrice = ResolveApiDailyPrice(apiRatePrice, vehicle);
                var apiTotalPrice = ResolveApiTotalPrice(apiRatePrice, vehicle);
                var apiOneWayFee = apiVehicle.prices?.price_definition?.one_way_fee ?? vehicle.OneWayFee;

                if (additionalInformation?.Agency != null)
                    CalculationHelper.CalculateFinalVehiclePrices(vendor, vehicle, additionalInformation.Agency, requestCurrencyType, profitMarkups, exchangeRates);

                vehicle.ReservationToken = CreateReservationToken(
                    apiVehicle,
                    vehicle,
                    getVehiclesRequest,
                    additionalInformation,
                    vendor,
                    requestCurrencyType,
                    baseVendorRequestCurrencyType,
                    planReference,
                    visitorSessionId,
                    apiDailyPrice,
                    apiTotalPrice,
                    apiOneWayFee,
                    apiRatePrice?.booking_token).ToJson();

                vehicles.Add(vehicle);
            }

            return vehicles;
        }

        public static List<Vehicle> Map(this List<ReservawayFilteredVehicleItem> apiVehicleList, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var vehicles = new List<Vehicle>();
            if (apiVehicleList == null)
                return vehicles;

            for (int i = 0; i < apiVehicleList.Count; i++)
            {
                var apiVehicle = apiVehicleList[i];
                var planDefinition = ReservawayMapperHelper.GetBasePlanDefinition(apiVehicle.prices);
                if (planDefinition == null)
                    continue;

                var ratePrice = ReservawayMapperHelper.GetRatePrice(apiVehicle.prices, planDefinition?.product_type_name, planDefinition?.payment_type_name);
                var totalPrice = ratePrice?.total_price > 0 ? ratePrice.total_price : apiVehicle.basePlan?.price ?? 0;
                var dailyPrice = additionalInformation.RentalDuration > 0 ? totalPrice / additionalInformation.RentalDuration : totalPrice;
                var vendorName = vendor.ShowSubVendorLogo == true ? apiVehicle.vendor?.name : vendor.VendorName;
                var vendorLogo = vendor.ShowSubVendorLogo == true ? apiVehicle.vendor?.logoUrl : vendor.Logo;

                vehicles.Add(new Vehicle
                {
                    VehicleId = i + 1,
                    VendorId = vendor.VendorId,
                    VendorName = vendorName.ToStringNullSafe(),
                    ApiVendorName = apiVehicle.vendor?.name.ToStringNullSafe(),
                    VendorPhone = vendor.VendorPhone,
                    VendorEmail = vendor.VendorEmail,
                    VendorLogo = vendorLogo.ToStringNullSafe(),
                    VehicleCode = apiVehicle.id,
                    VehicleName = apiVehicle.name.ToStringNullSafe(),
                    VehicleDescription = BuildVehicleDescription(apiVehicle),
                    DailyPrice = dailyPrice,
                    DailyPricePayNow = dailyPrice,
                    TotalPrice = totalPrice,
                    TotalPricePayNow = totalPrice,
                    OneWayFee = apiVehicle.options?.oneWayFee ?? 0,
                    ExtraPrice = 0,
                    RentalDuration = additionalInformation.RentalDuration,
                    IsAvailable = true,
                    CurrencyCode = apiVehicle.currency.ToStringNullSafe(),
                    PassangerQuantityType = MapPassengerQuantity(apiVehicle.seats),
                    PassangerQuantityName = apiVehicle.seats > 0 ? apiVehicle.seats.ToString() : string.Empty,
                    BaggageQuantityType = MapBaggageQuantity(apiVehicle.luggageCapacity ?? 0),
                    BaggageQuantityName = apiVehicle.luggageCapacity > 0 ? apiVehicle.luggageCapacity.ToString() : string.Empty,
                    FuelType = MapFuelType(apiVehicle.fuelType),
                    FuelTypeName = apiVehicle.fuelType.ToStringNullSafe(),
                    TransmissionType = MapTransmissionType(apiVehicle.transmission),
                    TransmissionTypeName = apiVehicle.transmission.ToStringNullSafe(),
                    VehicleCategoryType = MapVehicleCategory(apiVehicle.filterTags, apiVehicle.name),
                    VehicleCategoryTypeName = GetFilterLabel(apiVehicle.filterTags, "vehicle_").ToStringNullSafe(),
                    VehicleType = MapVehicleType(apiVehicle.filterTags, apiVehicle.name),
                    VehicleTypeName = GetFilterLabel(apiVehicle.filterTags, "vehicle_").ToStringNullSafe(),
                    IsThereAirCondition = true,
                    VehicleImages = string.IsNullOrWhiteSpace(apiVehicle.imgUrl)
                        ? new List<VehicleImage>()
                        : new List<VehicleImage> { new VehicleImage { Url = apiVehicle.imgUrl } },
                    PickupLocationId = additionalInformation.PickupLocationId,
                    PickupLocationCode = additionalInformation.APIPickupLocationCode,
                    PickupLocationName = additionalInformation.PickupLocationName,
                    ReturnLocationId = additionalInformation.ReturnLocationId,
                    ReturnLocationCode = additionalInformation.APIReturnLocationCode,
                    ReturnLocationName = additionalInformation.ReturnLocationName,
                    PickupDateTime = additionalInformation.PickupDateTime,
                    ReturnDateTime = additionalInformation.ReturnDateTime,
                    DepositPrice = ratePrice?.deposit_price > 0 ? ratePrice.deposit_price : apiVehicle.basePlan?.deposit,
                    VendorMinimumDriverAge = 0,
                    TotalKMLimit = apiVehicle.options?.mileageLimit?.total,
                    IsAirport = apiVehicle.options?.pickupType.ToStringNullSafe().Equals("inTerminal", StringComparison.OrdinalIgnoreCase) == true,
                    IsOffice = apiVehicle.options?.pickupType.ToStringNullSafe().Equals("office", StringComparison.OrdinalIgnoreCase) == true,
                    RentalWorkingTypes = vendor.RentalWorkingType,
                    ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
                });
            }

            return vehicles;
        }

        public static Vehicle Map(this ReservawayVehicleDetail apiVehicle, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, ReservawayPlanReference planReference)
        {
            if (apiVehicle == null)
                return null;

            var ratePrice = GetSelectedRatePrice(apiVehicle, planReference);
            var totalPrice = ratePrice?.total_price > 0 ? ratePrice.total_price : apiVehicle.cheapest_price.ToFloatNullSafe();
            var dailyPrice = additionalInformation.RentalDuration > 0 ? totalPrice / additionalInformation.RentalDuration : totalPrice;
            var vendorName = vendor.ShowSubVendorLogo == true ? apiVehicle.vendor?.name : vendor.VendorName;
            var vendorLogo = vendor.ShowSubVendorLogo == true ? apiVehicle.vendor?.logo : vendor.Logo;

            return new Vehicle
            {
                VehicleId = apiVehicle.kolay_id > 0 ? apiVehicle.kolay_id : apiVehicle.id.ToIntNullSafe(),
                VendorId = vendor.VendorId,
                VendorName = vendorName.ToStringNullSafe(),
                ApiVendorName = apiVehicle.vendor?.name.ToStringNullSafe(),
                VendorPhone = vendor.VendorPhone,
                VendorEmail = vendor.VendorEmail,
                VendorLogo = vendorLogo.ToStringNullSafe(),
                VehicleCode = apiVehicle.id,
                VehicleName = !string.IsNullOrWhiteSpace(apiVehicle.vehicle_name) ? apiVehicle.vehicle_name : apiVehicle.vehicle_model_name,
                VehicleDescription = BuildVehicleDescription(apiVehicle),
                DailyPrice = dailyPrice,
                DailyPricePayNow = dailyPrice,
                TotalPrice = totalPrice,
                TotalPricePayNow = totalPrice,
                OneWayFee = apiVehicle.prices?.price_definition?.one_way_fee ?? 0,
                ExtraPrice = apiVehicle.extras_prices?.total ?? 0,
                RentalDuration = additionalInformation.RentalDuration,
                IsAvailable = apiVehicle.is_available,
                CurrencyCode = GetFirstNonEmpty(apiVehicle.base_currency, apiVehicle.prices?.price_definition?.currency),
                PassangerQuantityType = MapPassengerQuantity(apiVehicle.seats),
                PassangerQuantityName = apiVehicle.passenger_quantity_type.ToStringNullSafe(),
                BaggageQuantityType = MapBaggageQuantityFromName(apiVehicle.baggage_quantity_type),
                BaggageQuantityName = apiVehicle.baggage_quantity_type.ToStringNullSafe(),
                FuelType = MapFuelType(apiVehicle.fuel_type),
                FuelTypeName = apiVehicle.fuel_type.ToStringNullSafe(),
                TransmissionType = MapTransmissionType(apiVehicle.transmission_type),
                TransmissionTypeName = apiVehicle.transmission_type.ToStringNullSafe(),
                VehicleCategoryType = MapVehicleCategory(apiVehicle.vehicle_category_type),
                VehicleCategoryTypeName = apiVehicle.vehicle_category_type.ToStringNullSafe(),
                VehicleType = MapVehicleType(apiVehicle.vehicle_type, apiVehicle.vehicle_category_type),
                VehicleTypeName = apiVehicle.vehicle_type.ToStringNullSafe(),
                SippCode = !string.IsNullOrWhiteSpace(apiVehicle.sipp_code) ? apiVehicle.sipp_code : apiVehicle.vehicle_code,
                IsThereAirCondition = apiVehicle.has_air_condition,
                VehicleImages = apiVehicle.vehicle_images?.Select(x => new VehicleImage { Url = x.url }).ToList() ?? new List<VehicleImage>(),
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationCode = additionalInformation.APIPickupLocationCode,
                PickupLocationName = !string.IsNullOrWhiteSpace(apiVehicle.pickup_location) ? apiVehicle.pickup_location : additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationCode = additionalInformation.APIReturnLocationCode,
                ReturnLocationName = !string.IsNullOrWhiteSpace(apiVehicle.dropoff_location) ? apiVehicle.dropoff_location : additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                DepositPrice = ratePrice?.deposit_price,
                VendorMinimumDriverAge = apiVehicle.vendor_minimum_driver_age,
                TotalKMLimit = apiVehicle.total_k_m_limit.ToIntNullSafe(),
                IsAirport = apiVehicle.is_airport,
                IsOffice = apiVehicle.is_office,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice,
                PickupLocationAddress = apiVehicle.pickup_location,
                ReturnLocationAddress = apiVehicle.dropoff_location
            };
        }

        public static ReservawayRatePrice GetSelectedRatePrice(ReservawayVehicleDetail vehicle, ReservawayPlanReference planReference)
        {
            if (vehicle?.prices == null)
                return null;

            var productName = !string.IsNullOrWhiteSpace(planReference?.ProductTypeName)
                ? planReference.ProductTypeName
                : ReservawayMapperHelper.GetBasePlanDefinition(vehicle.prices)?.product_type_name;
            var paymentName = !string.IsNullOrWhiteSpace(planReference?.PaymentTypeName)
                ? planReference.PaymentTypeName
                : ReservawayMapperHelper.GetBasePlanDefinition(vehicle.prices)?.payment_type_name;

            return ReservawayMapperHelper.GetRatePrice(vehicle.prices, productName, paymentName);
        }

        private static float ResolveApiDailyPrice(ReservawayRatePrice ratePrice, Vehicle vehicle)
        {
            if (ratePrice?.daily_price > 0)
                return ratePrice.daily_price;

            return vehicle?.DailyPrice ?? 0;
        }

        private static float ResolveApiTotalPrice(ReservawayRatePrice ratePrice, Vehicle vehicle)
        {
            if (ratePrice?.total_price > 0)
                return ratePrice.total_price;

            return vehicle?.TotalPrice ?? 0;
        }

        private static ReservationToken CreateReservationToken(
            ReservawayVehicleDetail apiVehicle,
            Vehicle vehicle,
            GetVehiclesRequest getVehiclesRequest,
            ResponseReservationStepsAdditionalInformation additionalInformation,
            Vendor vendor,
            CurrencyTypes requestCurrencyType,
            CurrencyTypes baseVendorRequestCurrencyType,
            ReservawayPlanReference planReference,
            string visitorSessionId,
            float apiDailyPrice,
            float apiTotalPrice,
            float apiOneWayFee,
            string bookingToken)
        {
            return new ReservationToken
            {
                AgencyId = additionalInformation.Agency?.AgencyId ?? 0,
                VendorId = vendor.VendorId,
                APIVendorId = apiVehicle.vendor_id > 0 ? apiVehicle.vendor_id : vendor.VendorId,
                APIVendorName = apiVehicle.vendor?.name ?? vendor.VendorName,
                APIVendorPhone = vendor.VendorPhone,
                APIVendorEmail = vendor.VendorEmail,
                APIVendorLogo = apiVehicle.vendor?.logo ?? vendor.Logo,
                VehicleId = vehicle.VehicleId,
                ApiVehicleId = apiVehicle.id.ToIntNullSafe(),
                VehicleCode = apiVehicle.id,
                APIPickupLocationId = additionalInformation.APIPickupLocationId,
                APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                APIReturnLocationId = additionalInformation.APIReturnLocationId,
                APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                CurrencyType = requestCurrencyType,
                RentalDuration = vehicle.RentalDuration,
                DailyPrice = vehicle.DailyPrice,
                OneWayFee = vehicle.OneWayFee,
                DailyPricePayNow = vehicle.DailyPricePayNow,
                APIDailyPrice = apiDailyPrice,
                APITotalPrice = apiTotalPrice,
                APITotalPricePayNow = apiTotalPrice,
                APIDailyPricePayNow = apiDailyPrice,
                APIOneWayFee = apiOneWayFee,
                APIReferenceCode = apiVehicle.search_hash,
                APIReferenceCode2 = ReservawayMapperHelper.CreatePlanReference(new ReservawayPlanDefinition
                {
                    product_type_id = planReference?.ProductTypeId ?? 0,
                    payment_type_id = planReference?.PaymentTypeId ?? 0,
                    product_type_name = planReference?.ProductTypeName,
                    payment_type_name = planReference?.PaymentTypeName
                }),
                APIReferenceCode3 = visitorSessionId,
                APIReferenceCode4 = bookingToken,
                DepositPrice = vehicle.DepositPrice,
                VendorMinimumDriverAge = vehicle.VendorMinimumDriverAge ?? 0,
                VendorMinimumDrivingLicenseAge = vehicle.VendorMinimumDrivingLicenseAge ?? 0,
                ServiceCharge = vehicle.ServiceCharge,
                FuelType = vehicle.FuelType,
                TransmissionType = vehicle.TransmissionType,
                VehicleCategoryType = vehicle.VehicleCategoryType,
                VehicleType = vehicle.VehicleType,
                PassangerQuantityType = vehicle.PassangerQuantityType,
                BaggageQuantityType = vehicle.BaggageQuantityType,
                DepositCreditCardRequired = vehicle.DepositCreditCardRequired,
                PickupLocationId = getVehiclesRequest.PickupLocationId,
                ReturnLocationId = getVehiclesRequest.ReturnLocationId,
                LanguageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>(),
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                VehicleName = vehicle.VehicleName,
                VehicleImageUrl = vehicle.VehicleImages?.Count > 0 ? vehicle.VehicleImages[0].Url : string.Empty,
                BaseVendorRequestCurrencyType = baseVendorRequestCurrencyType,
                SpecialProfitApplied = vehicle.SpecialProfitApplied,
                TotalKmLimit = vehicle.TotalKMLimit ?? 0,
                VendorFlightPassRequired = vendor.FlightNumberRequired ?? false,
                FullCredit = vehicle.FullCredit,
                SippCode = vehicle.SippCode,
                APIDeliveryTypeId = apiVehicle.delivery_type_id
            };
        }

        private static string BuildVehicleDescription(ReservawayFilteredVehicleItem vehicle)
            => string.Join(" - ", new[]
            {
                vehicle.vendor?.name,
                vehicle.options?.pickupType,
                vehicle.options?.fuelPolicy
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

        private static string BuildVehicleDescription(ReservawayVehicleDetail vehicle)
            => string.Join(" - ", new[]
            {
                vehicle.vendor?.name,
                vehicle.delivery_type,
                vehicle.vendor?.full_name
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

        private static string GetFirstNonEmpty(params string[] values)
            => values?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))?.Trim() ?? string.Empty;

        private static FuelTypes MapFuelType(string fuelType)
        {
            var normalized = fuelType.ToStringNullSafe().Trim().ToLowerInvariant();
            if (normalized.Contains("diesel"))
                return FuelTypes.Diesel;
            if (normalized.Contains("electric"))
                return FuelTypes.Electric;
            if (normalized.Contains("hybrid"))
                return FuelTypes.Hybrid;
            if (normalized.Contains("lpg"))
                return FuelTypes.GasolineAndLPG;
            if (normalized.Contains("gasoline") || normalized.Contains("petrol"))
                return FuelTypes.Gasoline;

            return FuelTypes.None;
        }

        private static TransmissionTypes MapTransmissionType(string transmission)
        {
            var normalized = transmission.ToStringNullSafe().Trim().ToLowerInvariant();
            if (normalized.Contains("automatic"))
                return TransmissionTypes.Automatic;
            if (normalized.Contains("manual"))
                return TransmissionTypes.Manuel;

            return TransmissionTypes.None;
        }

        private static VehicleCategoryTypes MapVehicleCategory(List<string> filterTags, string vehicleName)
            => MapVehicleCategory($"{GetFilterLabel(filterTags, "vehicle_")} {vehicleName}");

        private static VehicleCategoryTypes MapVehicleCategory(string value)
        {
            var normalized = value.ToStringNullSafe().Trim().ToLowerInvariant();
            if (normalized.Contains("small") || normalized.Contains("economic") || normalized.Contains("economy"))
                return VehicleCategoryTypes.Economic;
            if (normalized.Contains("medium") || normalized.Contains("intermediate"))
                return VehicleCategoryTypes.Intermediate;
            if (normalized.Contains("large") || normalized.Contains("full"))
                return VehicleCategoryTypes.FullSize;
            if (normalized.Contains("premium"))
                return VehicleCategoryTypes.Premium;
            if (normalized.Contains("luxury"))
                return VehicleCategoryTypes.Luxury;
            if (normalized.Contains("minivan"))
                return VehicleCategoryTypes.Minivan;
            if (normalized.Contains("suv"))
                return VehicleCategoryTypes.SUV;

            return VehicleCategoryTypes.None;
        }

        private static VehicleTypes MapVehicleType(List<string> filterTags, string vehicleName)
            => MapVehicleType($"{GetFilterLabel(filterTags, "vehicle_")} {vehicleName}", vehicleName);

        private static VehicleTypes MapVehicleType(string vehicleType, string fallback)
        {
            var normalized = $"{vehicleType} {fallback}".ToStringNullSafe().Trim().ToLowerInvariant();
            if (normalized.Contains("suv"))
                return VehicleTypes.SUV;
            if (normalized.Contains("sedan"))
                return VehicleTypes.Sedan;
            if (normalized.Contains("hatchback"))
                return VehicleTypes.FiveDoorHatchback;
            if (normalized.Contains("wagon"))
                return VehicleTypes.StationWagon;
            if (normalized.Contains("van") || normalized.Contains("minivan"))
                return VehicleTypes.Van;
            if (normalized.Contains("pickup"))
                return VehicleTypes.Pickup;

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
                11 => PassangerQuantityTypes.ElevenPerson,
                12 => PassangerQuantityTypes.TwelvePerson,
                _ => PassangerQuantityTypes.None
            };

        private static BaggageQuantityTypes MapBaggageQuantityFromName(string value)
        {
            var number = new string(value.ToStringNullSafe().Where(char.IsDigit).ToArray()).ToIntNullSafe();
            return MapBaggageQuantity(number);
        }

        private static BaggageQuantityTypes MapBaggageQuantity(int capacity)
            => capacity switch
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

        private static string GetFilterLabel(List<string> filterTags, string prefix)
            => filterTags?.FirstOrDefault(x => x.ToStringNullSafe().StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
    }
}
