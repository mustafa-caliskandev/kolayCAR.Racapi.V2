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
    public class ExtraProvider : IExtraProvider
    {
        private readonly HttpManager _httpManager;
        private readonly AuthProvider _authProvider;

        public ExtraProvider(string apiBaseUrl)
        {
            _httpManager = new HttpManager(YesOtoConstants.NormalizeApiBaseUrl(apiBaseUrl));
            _authProvider = new AuthProvider(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            var reservationToken = additionalInformation?.ReservationToken;

            if (reservationToken == null || string.IsNullOrWhiteSpace(reservationToken.VehicleCode))
                return new ServiceResponseBase(null, false, "Rezervasyon token veya arac kodu bulunamadi.");

            var accessToken = await _authProvider.GetTokenAsync(vendor);
            if (string.IsNullOrEmpty(accessToken))
                return new ServiceResponseBase(null, false, "Token bilgisi alinamadi!");

            var response = await _httpManager.PostAsyncWithModel<YesOtoSearchVehicleRequest, YesOtoVehicleListResponse>(
                "/api/app/reservationUI/findReservations",
                CreateSelectVehicleRequest(vendor, additionalInformation, reservationToken.VehicleCode),
                new Dictionary<string, object>(),
                CreateHeaders(accessToken)
            );

            if (response == null || !response.success || response.data == null)
                return new ServiceResponseBase(null, false, response?.message ?? "Ek urun listesi alinamadi.");

            var selectedApiVehicle = response.data.selectedVehicle
                                     ?? response.data.vehicles?.FirstOrDefault(vehicle => vehicle.vehicleGroup.id == reservationToken.VehicleCode);

            var selectedVehicle = selectedApiVehicle?.Map(additionalInformation, vendor);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false, "Arac bilgisi alinamadi.");

            selectedVehicle = MapLocalVehicleIfNeeded(selectedVehicle, reservationToken, vendor, exchangeRates, localVehicles);
            selectedVehicle.VehicleId = reservationToken.VehicleId > 0 ? reservationToken.VehicleId : selectedVehicle.VehicleId;
            selectedVehicle.VehicleCode = reservationToken.VehicleCode;

            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var extras = response.data.additionalServices.Map();

            selectedVehicle.Extras = extras;
            selectedVehicle.ReservationToken = reservationToken.ToJson();

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            var filteredExtras = ReservationHelper.RemoveZeroPriceExtras(extras) ?? new List<Extra>();

            return new ServiceResponseBase(new GetExtrasResponse(filteredExtras, selectedVehicle), true);
        }

        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var result = new List<Extra>();
            return await Task.FromResult(new ServiceResponseBase(result, true));
        }

        private static Vehicle MapLocalVehicleIfNeeded(Vehicle selectedVehicle, ReservationToken reservationToken, Vendor vendor, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles)
        {
            if (!vendor.VehicleMappingActive)
                return selectedVehicle;

            var localVehicle = localVehicles?.FirstOrDefault(vehicle => vehicle.VehicleCode == reservationToken.VehicleCode);

            return localVehicle == null
                ? selectedVehicle
                : VehicleHelper.MapLocalVehicle(
                    selectedVehicle,
                    localVehicle,
                    exchangeRates: exchangeRates,
                    sourceCurrenyType: vendor.CurrencyType,
                    targetCurrenyType: reservationToken.CurrencyType,
                    useLocalDeposit: vendor.UseLocalDeposit == true,
                    vendor: vendor);
        }

        private static YesOtoSearchVehicleRequest CreateSelectVehicleRequest(Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string vehicleCode)
        {
            return new YesOtoSearchVehicleRequest
            {
                BrandId = vendor.ApiClientId,
                SalesChannelId = YesOtoConstants.SalesChannelId,
                LanguageId = null,
                Location = additionalInformation.APIPickupLocationCode,
                DropOffLocation = additionalInformation.APIReturnLocationCode,
                Start = FormatApiDate(additionalInformation.PickupDateTime),
                End = FormatApiDate(additionalInformation.ReturnDateTime),
                Age = null,
                SelectedVehicleGroup = vehicleCode,
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
