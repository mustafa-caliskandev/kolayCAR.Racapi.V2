using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Ototur;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.OtoturResponseBase;

namespace KolayCAR.Broker.API.Providers.Ototur
{

    public class ExtraProvider : IExtraProvider
    {
        HttpManager _httpManager;
        AuthProvider _authProvider { get; set; }

        VehicleProvider _vehicleProvider;
        public ExtraProvider(string apiBaseUrl)
        {
            _httpManager = new HttpManager(apiBaseUrl);
            _authProvider = new AuthProvider(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var token = await _authProvider.GetToken();
            var result = await _httpManager.GetAsync2<OtoturExtraResponseBase>(
               requestPath: "extras",
               headers: GetHeaders(token)
               );
            if (result.Success)
            {
                if (result.Data.result.content.Count > 0)
                {
                    return new ServiceResponseBase
                    {
                        Data = result.Data.result.content.Map(vendor),
                        Success = true
                    };
                }
                return new ServiceResponseBase
                {
                    Data = null,
                    Success = false,
                    Message = result.Message
                };

            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Ototur servisine ulaşılamadı"
            };
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            _vehicleProvider = new VehicleProvider(vendor, false);
            var reservationToken = additionalInformation.ReservationToken;
            CurrencyTypes requestCurrencyType = (CurrencyTypes)Enum.Parse(typeof(CurrencyTypes), getExtrasRequest.CurrencyCode, true);

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await _vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == reservationToken.VehicleCode);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            var extras = selectedVehicle.Extras;

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle);
            return new ServiceResponseBase(getExtrasResponse, true);
        }
        private IDictionary<string, object> GetHeaders(OtoturAuthResponse ototurAuthResponse)
        {
            return new Dictionary<string, object>() { { "Authorization", $"Bearer {ototurAuthResponse.token}" } };
        }
    }
}
