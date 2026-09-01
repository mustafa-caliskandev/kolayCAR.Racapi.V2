using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.AutoHome;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.AutoHome
{
    public class ExtraProvider : IExtraProvider
    {

        private readonly HttpManager _httpmanager;
        private VehicleProvider _vehicleProvider;
        public ExtraProvider(string apiBaseUrl)
        {
            _httpmanager = new HttpManager(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var result = await _httpmanager.GetAsync2<List<AutoHomeResponseBase.ListExtra>>(
               requestPath: "/API/reservation/v2/GetAdditionalProducts.php",
               parameters: GetExtraParameters(vendor)
               ); ;
            if (result.Success)
            {
                if (result.Data.Count > 0)
                {
                    return new ServiceResponseBase
                    {
                        Data = result.Data.Map(vendor),
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
                Message = "AutoHome servisine ulaşılamadı"
            };
        }


        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            _vehicleProvider = new VehicleProvider(vendor, false);
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await _vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == reservationToken.VehicleCode);

            if (getVehiclesResponse?.Data == null)
                return new ServiceResponseBase(null, false);

            var extras = selectedVehicle.Extras;

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle, vehicles);
            return new ServiceResponseBase(getExtrasResponse, true);
        }
        private IDictionary<string, object> GetExtraParameters(Vendor vendor)
        {
            return new Dictionary<string, object>() { { "login", vendor.ApiKey }, { "passwd", vendor.ApiPassword } };
        }
    }
}
