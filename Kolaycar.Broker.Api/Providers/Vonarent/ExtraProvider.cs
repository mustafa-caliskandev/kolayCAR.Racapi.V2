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
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Vonarent
{
    public class ExtraProvider : IExtraProvider
    {
        private readonly VehicleProvider _vehicleProvider;
        private readonly HttpManager _httpManager;
        private readonly AuthProvider _authProvider;
        private const string ExtrasPath = "/api/remote/v1/extra/extras";

        public ExtraProvider(Vendor vendor)
        {
            _vehicleProvider = new VehicleProvider(vendor, false);
            _authProvider = new AuthProvider(vendor.APIBaseUrl, vendor.APITimeout);
            _httpManager = new HttpManager(vendor.APIBaseUrl, timeout: vendor.APITimeout);
        }

        public async Task<ServiceResponseBase> GetExtras(
            GetExtrasRequest getExtrasRequest,
            Vendor vendor,
            ResponseReservationStepsAdditionalInformation additionalInformation,
            List<ExchangeRates> exchangeRates,
            List<Vehicle> localVehicles,
            List<SubVendor> subVendors,
            bool addProfitMarkup = true,
            bool getAPIPrices = false)
        {
            var reservationToken = additionalInformation?.ReservationToken;
            if (reservationToken == null)
                return new ServiceResponseBase(null, false, $"{vendor.VendorName} ekstra istegi icin reservation token bulunamadi.");

            var bearerToken = reservationToken.APIReferenceCode;
            if (string.IsNullOrWhiteSpace(bearerToken))
                return new ServiceResponseBase(null, false, $"{vendor.VendorName} ekstra istegi icin bearer token bulunamadi.");

            var selectVehicleResult = await _vehicleProvider.SelectVehicleAsync(vendor, reservationToken, bearerToken);
            if (!selectVehicleResult.Success)
                return selectVehicleResult;

            var headers = _authProvider.CreateAuthorizedHeaders(vendor, bearerToken, HttpMethod.Get.Method, ExtrasPath);
            var result = await _httpManager.GetAsync2<VonarentExtraResponse>(
                requestPath: ExtrasPath,
                headers: headers,
                isReservationRequest: true);

            if (result?.Data?.status != 1 || result.Data.items == null)
            {
                return new ServiceResponseBase(
                    data: result?.Data,
                    success: false,
                    message: $"{vendor.VendorName} ekstra servisi basarisiz.",
                    serviceMessage: VonarentResponseMessageHelper.ExtractMessage(result?.Data, result?.ServiceMessage, result?.Message));
            }

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);
            var getVehiclesResponse = await _vehicleProvider.GetVehiclesByBearerTokenAsync(
                getVehiclesRequest,
                vendor,
                additionalInformation,
                exchangeRates,
                localVehicles,
                subVendors,
                reservationToken.BaseVendorRequestCurrencyType,
                bearerToken);

            if (!getVehiclesResponse.Success)
                return getVehiclesResponse;

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == reservationToken.VehicleCode);
            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false, $"{vendor.VendorName} secili arac bilgisi alinamadi.");

            var extras = result.Data.items.Map();
            selectedVehicle.Extras = extras;

            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            extras = ReservationHelper.RemoveZeroPriceExtras(extras);
            var getExtrasResponse = new GetExtrasResponse(extras, selectedVehicle);
            return new ServiceResponseBase(getExtrasResponse, true);
        }

        public async Task<ServiceResponseBase> GetExtraList(
            Vendor vendor,
            CurrencyTypes currencyType,
            LanguageTypes languageType,
            int rentalDuration)
            => new ServiceResponseBase(null, false, "Vonarent ekstra listesi icin once arac secimi yapilmasi gerekiyor.");
    }
}
