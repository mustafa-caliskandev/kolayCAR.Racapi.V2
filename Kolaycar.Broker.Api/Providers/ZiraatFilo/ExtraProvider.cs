using KolayCAR.Broker.API.Mappers.ZiraatFilo;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.API.Helpers;
namespace KolayCAR.Broker.API.Providers.ZiraatFilo
{
    public class ExtraProvider : IExtraProvider
    {
        private HttpManager _httpManager;
        private AuthProvider _authProvider;
        private IVehicleProvider _vehicleProvider;
        public ExtraProvider(string apiBaseUrl)
        {
            _httpManager = new HttpManager(apiBaseUrl);
            _authProvider = new AuthProvider(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var headers = await _authProvider.GetHeaders(vendor);
            if (headers == null)
                return new (null, false, "Token bilgisi alınamadı!");

          var extras = await _httpManager.GetXmlAsync<ZiraatFiloResposeBase.Root>(
                requestPath: "/api/broker-service/insurance/",
                headers: headers);

            if (extras == null)
                return new (null, false, "Tedarikçi servisinden yanıt alınamadı!");

            return new (extras.Turevsistem.MesajBilgi.Map(vendor), true);
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {   
            _vehicleProvider = new VehicleProvider(vendor, false);
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();        

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await _vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleId == additionalInformation.VehicleId);

            if (selectedVehicle == null)
                return new (null, false);

            var extras = selectedVehicle.Extras;

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle);
            return new (getExtrasResponse, true);
        }
    }
}
