using Kolaycar.Broker.Api.Helpers.YesOto;
using Kolaycar.Broker.Api.Mappers.YesOto;
using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Requests.YesOto;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Responses.YesOto;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Kolaycar.Broker.Api.Providers.YesOto
{
    public class VehicleProvider : IVehicleProvider
    {
        private readonly HttpManager _httpManager;
        private readonly AuthProvider _authProvider;

        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            _httpManager = new HttpManager(YesOtoConstants.NormalizeApiBaseUrl(vendor.APIBaseUrl), timeout: disableTimeout ? 0 : vendor.APITimeout);
            _authProvider = new AuthProvider(vendor.APIBaseUrl);
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var accessToken = await _authProvider.GetTokenAsync(vendor);

            if (string.IsNullOrEmpty(accessToken))
                return new ServiceResponseBase(null, false, "Token bilgisi alinamadi!");

            var response = await _httpManager.PostAsyncWithModel<YesOtoAgeGroupListResponse>(
                "/api/app/ageGroup/ageGroupList",
                new Dictionary<string, object>(),
                CreateHeaders(accessToken)
            );

            if (response != null)
                return new ServiceResponseBase(response, true);

            return new ServiceResponseBase(null, false, "Yas gruplari alinamadi!");
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var accessToken = await _authProvider.GetTokenAsync(vendor);

            if (string.IsNullOrEmpty(accessToken))
                return new ServiceResponseBase(null, false, "Token bilgisi alinamadi!");

            var response = await _httpManager.PostAsyncWithModel<YesOtoSearchVehicleRequest, YesOtoVehicleListResponse>(
                "/api/app/reservationUI/findReservations",
                CreateSearchVehicleRequest(vendor, additionalInformation),
                new Dictionary<string, object>(),
                CreateHeaders(accessToken)
            );

            if (response == null || !response.success || response.data?.vehicles == null || response.data.vehicles.Count == 0)
            {
                return new ServiceResponseBase
                {
                    Success = false,
                    Message = response?.message ?? "Arac bulunamadi."
                };
            }

            var apiVehicleList = VehicleHelper.SelectCheapestByGroup(
                response.data.vehicles.Where(vehicle => !string.IsNullOrWhiteSpace(vehicle.vehicleGroup.id)).ToList(),
                vehicle => vehicle.vehicleGroup.id,
                vehicle => vehicle.GetFinalTotal());

            var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var mappedVehicleList = apiVehicleList.Map(additionalInformation, vendor);

            if (vendor.VehicleMappingActive)
            {
                var localVehicleList = localVehicles ?? new List<Vehicle>();

                mappedVehicleList = VehicleHelper.MapLocalVehicleList(
                    mappedVehicleList,
                    localVehicleList,
                    exchangeRates: exchangeRates,
                    vendor.CurrencyType,
                    requestCurrencyType,
                    useLocalDeposit: vendor.UseLocalDeposit == true,
                    vendor: vendor);

                apiVehicleList.RemoveAll(apiVehicle => !localVehicleList.Any(localVehicle => localVehicle.VehicleCode == apiVehicle.vehicleGroup.id));
            }

            CalculationHelper.SetVehiclesPrices(mappedVehicleList, vendor, exchangeRates, requestCurrencyType, baseVendorRequestCurrencyType);
            VehicleHelper.SetVehiclesProperties(mappedVehicleList, vendor, additionalInformation.Agency, exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, profitMarkups);

            if (mappedVehicleList.Any(vehicle => Math.Abs(vehicle.RentalDuration - additionalInformation.RentalDuration) > 1))
                return new ServiceResponseBase(null, false, "Yanlis gun sayisi");

            SetReservationTokens(apiVehicleList, mappedVehicleList, getVehiclesRequest, vendor, additionalInformation, requestCurrencyType, baseVendorRequestCurrencyType);

            return new ServiceResponseBase(mappedVehicleList, mappedVehicleList.Count > 0);
        }

        private static void SetReservationTokens(List<YesOtoVehicleData> apiVehicleList, List<Vehicle> mappedVehicleList, GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, CurrencyTypes requestCurrencyType, CurrencyTypes baseVendorRequestCurrencyType)
        {
            var pickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime);
            var returnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime);
            var languageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>();

            foreach (var vehicle in apiVehicleList)
            {
                var vehicleCode = vehicle.vehicleGroup.id;
                var tempMappedVehicleList = mappedVehicleList.Where(mappedVehicle => mappedVehicle.VehicleCode == vehicleCode).ToList();

                foreach (var mappedVehicle in tempMappedVehicleList)
                {
                    var apiTotalPrice = vehicle.GetFinalTotal();
                    var apiOneWayFee = vehicle.discountedOneDirectionPrice > 0 ? vehicle.discountedOneDirectionPrice : vehicle.oneDirectionPrice;
                    var apiRentTotal = vehicle.totalDiscountedPrice > 0 ? vehicle.totalDiscountedPrice : apiTotalPrice - apiOneWayFee;
                    var apiDailyPrice = vehicle.discountedPricePerDay > 0
                        ? vehicle.discountedPricePerDay
                        : apiRentTotal / (mappedVehicle.RentalDuration > 0 ? mappedVehicle.RentalDuration : 1);

                    var reservationToken = new ReservationToken
                    {
                        AgencyId = additionalInformation.Agency.AgencyId,
                        VendorId = vendor.VendorId,
                        APIVendorId = vendor.VendorId,
                        APIVendorName = vendor.VendorName,
                        APIVendorPhone = vendor.VendorPhone,
                        APIVendorEmail = vendor.VendorEmail,
                        APIVendorLogo = vendor.Logo,
                        VehicleId = mappedVehicle.VehicleId,
                        VehicleCode = vehicleCode,
                        APIPickupLocationId = additionalInformation.APIPickupLocationId,
                        APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                        APIReturnLocationId = additionalInformation.APIReturnLocationId,
                        APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                        CurrencyType = requestCurrencyType,
                        RentalDuration = mappedVehicle.RentalDuration,
                        DailyPrice = mappedVehicle.DailyPrice,
                        OneWayFee = mappedVehicle.OneWayFee,
                        DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                        APIDailyPrice = apiDailyPrice,
                        APIDailyPricePayNow = apiDailyPrice,
                        APITotalPrice = apiTotalPrice,
                        APITotalPricePayNow = apiTotalPrice,
                        APIOneWayFee = apiOneWayFee,
                        APIReferenceCode = vehicle.id,
                        APIReferenceCode2 = vehicle.priceRuleId,
                        APIReferenceCode3 = apiTotalPrice.ToString(CultureInfo.InvariantCulture),
                        DepositPrice = mappedVehicle.DepositPrice,
                        VendorMinimumDriverAge = mappedVehicle.VendorMinimumDriverAge ?? 0,
                        VendorMinimumDrivingLicenseAge = mappedVehicle.VendorMinimumDrivingLicenseAge ?? 0,
                        ServiceCharge = mappedVehicle.ServiceCharge,
                        FuelType = mappedVehicle.FuelType,
                        TransmissionType = mappedVehicle.TransmissionType,
                        DepositCreditCardRequired = mappedVehicle.DepositCreditCardRequired,
                        PickupLocationId = getVehiclesRequest.PickupLocationId,
                        ReturnLocationId = getVehiclesRequest.ReturnLocationId,
                        LanguageType = languageType,
                        PickupDateTime = pickupDateTime,
                        ReturnDateTime = returnDateTime,
                        VehicleName = mappedVehicle.VehicleName,
                        VehicleImageUrl = mappedVehicle.VehicleImages?.Count > 0 ? mappedVehicle.VehicleImages[0].Url : string.Empty,
                        BaseVendorRequestCurrencyType = baseVendorRequestCurrencyType,
                        SpecialProfitApplied = mappedVehicle.SpecialProfitApplied,
                        BaggageQuantityType = mappedVehicle.BaggageQuantityType,
                        PassangerQuantityType = mappedVehicle.PassangerQuantityType,
                        TotalKmLimit = mappedVehicle.TotalKMLimit ?? 0,
                        VehicleCategoryType = mappedVehicle.VehicleCategoryType,
                        VehicleType = mappedVehicle.VehicleType,
                        VendorFlightPassRequired = vendor.FlightNumberRequired ?? false,
                        FullCredit = mappedVehicle.FullCredit,
                        SippCode = mappedVehicle.SippCode
                    };

                    mappedVehicle.ReservationToken = reservationToken.ToJson();

                    if (additionalInformation.Agency.SpecialParameters)
                    {
                        mappedVehicle.SpecialVendorId = vendor.VendorId.ToString();
                        mappedVehicle.SpecialVendorName = vendor.VendorName;
                        mappedVehicle.SpecialVendorLogo = vendor.Logo;
                    }
                }
            }
        }

        private static YesOtoSearchVehicleRequest CreateSearchVehicleRequest(Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation)
        {
            return new YesOtoSearchVehicleRequest
            {
                BrandId = vendor.ApiClientId,
                SalesChannelId = vendor.SecretKey,
                LanguageId = null,
                Location = additionalInformation.APIPickupLocationCode,
                DropOffLocation = additionalInformation.APIReturnLocationCode,
                Start = FormatApiDate(additionalInformation.PickupDateTime),
                End = FormatApiDate(additionalInformation.ReturnDateTime),
                Age = null,
                SelectedVehicleGroup = null,
                SelectedAdditionalServices = null,
                UserCampaignId = null,
                IsUserFirstReservation = false,
                DiscPrice = 0,
                GiftCoupon = null,
                LocationPay = false,
                ProcessType = "Extras",
                PromotionToken = null,
                RequestType = null,
                UsedPointState = false,
                ZubizuSaleId = null,
                priceMatrix = null
            };
        }

        private static Dictionary<string, object> CreateHeaders(string accessToken)
        {
            return new Dictionary<string, object>
            {
                { "Content-Type", "application/json" },
                { "Authorization", $"Bearer {accessToken}" }
            };
        }

        private static string FormatApiDate(DateTime dateTime)
        {
            return dateTime.ToString("MM/dd/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
        }
    }
}
