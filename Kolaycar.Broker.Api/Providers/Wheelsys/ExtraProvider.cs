using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Wheelsys;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Wheelsys
{
    public class ExtraProvider : IExtraProvider
    {
        public HttpManager _httpManager;
        public VehicleProvider _vehicleProvider;
        public ExtraProvider(string apiBaseUrl)
        {
            _httpManager = new HttpManager(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var result = await _httpManager.GetXmlAsync<WheelsysResponseBase.Root>(
            requestPath: $"{vendor.ApiKey}/link/v3/options_{vendor.ApiPassword.Split('-')[0]}.html",
            parameters: GetExtrasListRequestParameters(vendor));

            if (result != null && result.response != null)
            {
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = result.response.option.Map(vendor)
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = $"{vendor.VendorName} servisine ulaşılamadı!"
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

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle, vehicles);
            return new ServiceResponseBase(getExtrasResponse, true);
        }
        private Dictionary<string, object> GetExtrasListRequestParameters(Vendor vendor) =>
         new Dictionary<string, object>()
         {
                { "AGENT",  vendor.ApiPassword.Split('-')[1] }
         };
    }
}
