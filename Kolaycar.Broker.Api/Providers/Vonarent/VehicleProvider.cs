using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Vonarent;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Response.Vonarent;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Vonarent
{
    public class VehicleProvider : IVehicleProvider
    {
        private readonly HttpManager _httpManager;
        private readonly AuthProvider _authProvider;
        private readonly LocationProvider _locationProvider;
        private const string VehiclesPath = "/api/remote/v1/vehicle/vehicles";
        private const string SelectVehiclePath = "/api/remote/v1/vehicle/select-vehicle";

        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            var timeout = disableTimeout ? 0 : vendor.APITimeout;
            _httpManager = new HttpManager(vendor.APIBaseUrl, timeout: timeout);
            _authProvider = new AuthProvider(vendor.APIBaseUrl, timeout);
            _locationProvider = new LocationProvider(vendor.APIBaseUrl, timeout);
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
            => new ServiceResponseBase(null, false, "Vonarent arac listesi icin once lokasyon ve tarih secimi yapilmasi gerekiyor.");

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var bearerToken = await _authProvider.GetTokenAsync(vendor);

            if (string.IsNullOrWhiteSpace(bearerToken))
                return new ServiceResponseBase(null, false, $"{vendor.VendorName} yetkilendirme tokeni alinamadi.");

            var selectStationResult = await _locationProvider.SelectStationAsync(vendor, additionalInformation, bearerToken);
            if (!selectStationResult.Success)
                return selectStationResult;

            var headers = _authProvider.CreateAuthorizedHeaders(vendor, bearerToken, HttpMethod.Get.Method, VehiclesPath);
            var result = await _httpManager.GetAsync2<VonarentVehicleResponse>(
                requestPath: VehiclesPath,
                headers: headers,
                isReservationRequest: true);

            if (result?.Data?.status != 1 || result.Data.items == null || result.Data.items.Count == 0)
            {
                return new ServiceResponseBase(
                    data: result?.Data,
                    success: false,
                    message: $"{vendor.VendorName} servisinden arac bulunamadi!",
                    serviceMessage: VonarentResponseMessageHelper.ExtractMessage(result?.Data, result?.ServiceMessage, result?.Message));
            }

            var apiVehicleList = VehicleHelper.SelectCheapestByGroup(
                result.Data.items.Where(x => x != null && !string.IsNullOrWhiteSpace(x.id)).ToList(),
                x => x.id,
                x => x.priceDaily);

            var mappedVehicleList = apiVehicleList.Map(additionalInformation, vendor);
            var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var apiCurrencyType = VehicleMapper.GetCurrencyType(apiVehicleList.FirstOrDefault()?.currency, baseVendorRequestCurrencyType);
            var localVehicleList = localVehicles ?? new List<Vehicle>();

            if (vendor.VehicleMappingActive)
            {
                mappedVehicleList = VehicleHelper.MapLocalVehicleList(
                    mappedVehicleList,
                    localVehicleList,
                    exchangeRates: exchangeRates,
                    sourceCurrenyType: apiCurrencyType,
                    targetCurrenyType: requestCurrencyType,
                    useLocalDeposit: vendor.UseLocalDeposit ?? false,
                    vendor: vendor);

                apiVehicleList.RemoveAll(x => !localVehicleList.Any(e => e.VehicleCode == x.id));
            }

            CalculationHelper.SetVehiclesPrices(mappedVehicleList, vendor, exchangeRates, requestCurrencyType, apiCurrencyType);
            VehicleHelper.SetVehiclesProperties(mappedVehicleList, vendor, additionalInformation.Agency, exchangeRates, apiCurrencyType, requestCurrencyType, profitMarkups);

            var languageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>();

            foreach (var apiVehicle in apiVehicleList)
            {
                var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == apiVehicle.id).ToList();

                foreach (var mappedVehicle in tempMappedVehicleList)
                {
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
                        VehicleCode = apiVehicle.id,
                        APIPickupLocationId = additionalInformation.APIPickupLocationId,
                        APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                        APIReturnLocationId = additionalInformation.APIReturnLocationId,
                        APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                        CurrencyType = requestCurrencyType,
                        RentalDuration = mappedVehicle.RentalDuration,
                        DailyPrice = mappedVehicle.DailyPrice,
                        OneWayFee = mappedVehicle.OneWayFee,
                        DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                        APIDailyPrice = apiVehicle.priceDaily,
                        APITotalPrice = apiVehicle.price,
                        APIDailyPricePayNow = apiVehicle.priceDaily,
                        APIOneWayFee = apiVehicle.dropPrice,
                        APIReferenceCode = bearerToken,
                        APIReferenceCode2 = apiVehicle.currency,
                        DepositPrice = mappedVehicle.DepositPrice,
                        VendorMinimumDriverAge = mappedVehicle.VendorMinimumDriverAge ?? 0,
                        VendorMinimumDrivingLicenseAge = mappedVehicle.VendorMinimumDrivingLicenseAge ?? 0,
                        ServiceCharge = mappedVehicle.ServiceCharge,
                        FuelType = mappedVehicle.FuelType,
                        TransmissionType = mappedVehicle.TransmissionType,
                        VehicleCategoryType = mappedVehicle.VehicleCategoryType,
                        VehicleType = mappedVehicle.VehicleType,
                        PassangerQuantityType = mappedVehicle.PassangerQuantityType,
                        BaggageQuantityType = mappedVehicle.BaggageQuantityType,
                        DepositCreditCardRequired = mappedVehicle.DepositCreditCardRequired,
                        PickupLocationId = getVehiclesRequest.PickupLocationId,
                        ReturnLocationId = getVehiclesRequest.ReturnLocationId,
                        LanguageType = languageType,
                        PickupDateTime = additionalInformation.PickupDateTime,
                        ReturnDateTime = additionalInformation.ReturnDateTime,
                        VehicleName = mappedVehicle.VehicleName,
                        VehicleImageUrl = mappedVehicle.VehicleImages?.Count > 0 ? mappedVehicle.VehicleImages[0].Url : string.Empty,
                        BaseVendorRequestCurrencyType = apiCurrencyType,
                        SpecialProfitApplied = mappedVehicle.SpecialProfitApplied,
                        TotalKmLimit = mappedVehicle.TotalKMLimit ?? 0,
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

            return new ServiceResponseBase(mappedVehicleList, mappedVehicleList.Count > 0);
        }

        public async Task<ServiceResponseBase> GetVehiclesByBearerTokenAsync(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, string bearerToken, List<ProfitMarkup> profitMarkups = null)
        {
            if (string.IsNullOrWhiteSpace(bearerToken))
                return new ServiceResponseBase(null, false, $"{vendor.VendorName} yetkilendirme tokeni alinamadi.");

            var headers = _authProvider.CreateAuthorizedHeaders(vendor, bearerToken, HttpMethod.Get.Method, VehiclesPath);
            var result = await _httpManager.GetAsync2<VonarentVehicleResponse>(
                requestPath: VehiclesPath,
                headers: headers,
                isReservationRequest: true);

            if (result?.Data?.status != 1 || result.Data.items == null || result.Data.items.Count == 0)
            {
                return new ServiceResponseBase(
                    data: result?.Data,
                    success: false,
                    message: $"{vendor.VendorName} servisinden arac bulunamadi!",
                    serviceMessage: VonarentResponseMessageHelper.ExtractMessage(result?.Data, result?.ServiceMessage, result?.Message));
            }

            var apiVehicleList = VehicleHelper.SelectCheapestByGroup(
                result.Data.items.Where(x => x != null && !string.IsNullOrWhiteSpace(x.id)).ToList(),
                x => x.id,
                x => x.priceDaily);

            var mappedVehicleList = apiVehicleList.Map(additionalInformation, vendor);
            var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var apiCurrencyType = VehicleMapper.GetCurrencyType(apiVehicleList.FirstOrDefault()?.currency, baseVendorRequestCurrencyType);
            var localVehicleList = localVehicles ?? new List<Vehicle>();

            if (vendor.VehicleMappingActive)
            {
                mappedVehicleList = VehicleHelper.MapLocalVehicleList(
                    mappedVehicleList,
                    localVehicleList,
                    exchangeRates: exchangeRates,
                    sourceCurrenyType: apiCurrencyType,
                    targetCurrenyType: requestCurrencyType,
                    useLocalDeposit: vendor.UseLocalDeposit ?? false,
                    vendor: vendor);

                apiVehicleList.RemoveAll(x => !localVehicleList.Any(e => e.VehicleCode == x.id));
            }

            CalculationHelper.SetVehiclesPrices(mappedVehicleList, vendor, exchangeRates, requestCurrencyType, apiCurrencyType);
            VehicleHelper.SetVehiclesProperties(mappedVehicleList, vendor, additionalInformation.Agency, exchangeRates, apiCurrencyType, requestCurrencyType, profitMarkups);

            var languageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>();

            foreach (var apiVehicle in apiVehicleList)
            {
                var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == apiVehicle.id).ToList();

                foreach (var mappedVehicle in tempMappedVehicleList)
                {
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
                        VehicleCode = apiVehicle.id,
                        APIPickupLocationId = additionalInformation.APIPickupLocationId,
                        APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                        APIReturnLocationId = additionalInformation.APIReturnLocationId,
                        APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                        CurrencyType = requestCurrencyType,
                        RentalDuration = mappedVehicle.RentalDuration,
                        DailyPrice = mappedVehicle.DailyPrice,
                        OneWayFee = mappedVehicle.OneWayFee,
                        DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                        APIDailyPrice = apiVehicle.priceDaily,
                        APITotalPrice = apiVehicle.price,
                        APIDailyPricePayNow = apiVehicle.priceDaily,
                        APIOneWayFee = mappedVehicle.OneWayFee,
                        APIReferenceCode = bearerToken,
                        APIReferenceCode2 = apiVehicle.currency,
                        DepositPrice = mappedVehicle.DepositPrice,
                        VendorMinimumDriverAge = mappedVehicle.VendorMinimumDriverAge ?? 0,
                        VendorMinimumDrivingLicenseAge = mappedVehicle.VendorMinimumDrivingLicenseAge ?? 0,
                        ServiceCharge = mappedVehicle.ServiceCharge,
                        FuelType = mappedVehicle.FuelType,
                        TransmissionType = mappedVehicle.TransmissionType,
                        VehicleCategoryType = mappedVehicle.VehicleCategoryType,
                        VehicleType = mappedVehicle.VehicleType,
                        PassangerQuantityType = mappedVehicle.PassangerQuantityType,
                        BaggageQuantityType = mappedVehicle.BaggageQuantityType,
                        DepositCreditCardRequired = mappedVehicle.DepositCreditCardRequired,
                        PickupLocationId = getVehiclesRequest.PickupLocationId,
                        ReturnLocationId = getVehiclesRequest.ReturnLocationId,
                        LanguageType = languageType,
                        PickupDateTime = additionalInformation.PickupDateTime,
                        ReturnDateTime = additionalInformation.ReturnDateTime,
                        VehicleName = mappedVehicle.VehicleName,
                        VehicleImageUrl = mappedVehicle.VehicleImages?.Count > 0 ? mappedVehicle.VehicleImages[0].Url : string.Empty,
                        BaseVendorRequestCurrencyType = apiCurrencyType,
                        SpecialProfitApplied = mappedVehicle.SpecialProfitApplied,
                        TotalKmLimit = mappedVehicle.TotalKMLimit ?? 0,
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

            return new ServiceResponseBase(mappedVehicleList, mappedVehicleList.Count > 0);
        }

        public async Task<ServiceResponseBase> SelectVehicleAsync(Vendor vendor, ReservationToken reservationToken, string bearerToken = null)
        {
            if (reservationToken == null)
                return new ServiceResponseBase(null, false, $"{vendor?.VendorName ?? "Vonarent"} arac secim istegi icin reservation token bulunamadi.");

            bearerToken ??= reservationToken.APIReferenceCode;
            if (string.IsNullOrWhiteSpace(bearerToken))
                return new ServiceResponseBase(null, false, $"{vendor?.VendorName ?? "Vonarent"} arac secim istegi icin bearer token bulunamadi.");

            var request = reservationToken.MapToSelectVehicleRequest();

            var headers = _authProvider.CreateAuthorizedHeaders(vendor, bearerToken, HttpMethod.Post.Method, SelectVehiclePath, body: request);
            var content = new StringContent(SignatureHelper.SerializeBody(request), Encoding.UTF8, "application/json");
            var result = await _httpManager.PostAsync<VonarentStatusResponse>(
                requestPath: SelectVehiclePath,
                content: content,
                headers: headers,
                isReservationRequest: true);

            if (result?.Data?.status != 1)
            {
                return new ServiceResponseBase(
                    data: result?.Data,
                    success: false,
                    message: $"{vendor.VendorName} arac secim servisi basarisiz.",
                    serviceMessage: VonarentResponseMessageHelper.ExtractMessage(result?.Data, result?.ServiceMessage, result?.Message));
            }

            return new ServiceResponseBase(result.Data, true);
        }
    }
}
