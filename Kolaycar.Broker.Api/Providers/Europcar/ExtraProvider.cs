using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Europcar;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Europcar
{
    public class ExtraProvider : IExtraProvider
    {
        IVehicleProvider vehicleProvider;

        public ExtraProvider(string apiBaseUrl)
        {
        }

        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            //var result = await HttpManager.GetXmlAsync<EuropcarResponseBase>(
            //requestPath: $"https://www.europcar.com.tr/options.xml");

            var _httpManager = new HttpManager(vendor.SecretKey);
            var result = await _httpManager.GetAsyncWithModel<EuropcarExtraResponse>($"/broker/service/account/{vendor.ApiClientId}?limit=all");

            if (result?.data?.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = result.data.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Europcar servisine ulaşılamadı!"
            };
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            vehicleProvider = new VehicleProvider(vendor, false);
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleId == additionalInformation.VehicleId);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            var extras = selectedVehicle.Extras;

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle);
            return new ServiceResponseBase(getExtrasResponse, true);
        }

        private Dictionary<string, object> GetExtrasListRequestParameters(Vendor vendor) =>
            new Dictionary<string, object>()
            {
                { "AGENT",  vendor.ApiPassword.Split('-')[1] },
                { "CDP",  !string.IsNullOrEmpty(vendor.ApiClientId) ? vendor.ApiClientId : ""}
            };
    }
}
