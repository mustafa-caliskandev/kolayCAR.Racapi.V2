using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Circular2;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.Circular2ResponseBase;

namespace KolayCAR.Broker.API.Providers.Circular2
{
    public class ExtraProvider : IExtraProvider
    {
        IVehicleProvider vehicleProvider;
        private readonly HttpManager _httpManager;
        private readonly AuthProvider _authProvider;
        public ExtraProvider(string apiBaseUrl)
        {
            _httpManager = new HttpManager(apiBaseUrl);
            _authProvider = new AuthProvider(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var token = await _authProvider.GetTokenAsync(vendor.ApiKey);
            var extraResult = await _httpManager.GetAsync2<List<ExtraResponse>>(
                requestPath: $"/contract/extra/list?token={token.token}&size={50}&referralagent={vendor.ApiPassword}");
            if (extraResult != null)
            {
                if (extraResult.Data.Count > 0)
                {
                    return new ServiceResponseBase
                    {
                        Success = true,
                        Data = extraResult.Data.Map(vendor)
                    };
                }
                else
                    return new ServiceResponseBase
                    {
                        Success = false,
                        Data = null,
                        ServiceMessage = "Servisten extra listesi boş dönmüştür!"
                    };
            }
            else return new ServiceResponseBase
            {
                Success = false,
                Data = null,
                ServiceMessage = "Circular servisine ulaşılamamıştır!"
            };
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            vehicleProvider = new VehicleProvider(vendor, false);
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var requestLanguageType = getExtrasRequest.LanguageCode.ToEnum<LanguageTypes>();

            var auth = await _authProvider.GetTokenAsync(vendor.ApiKey);
            var result = await _httpManager.GetAsync2<List<ExtraResponse>>(
                requestPath: $"/contract/extra/list?token={auth.token}&size={50}&referralagent={vendor.ApiPassword}");
            
            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == reservationToken.VehicleCode);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            var extras = result?.Data?.Map(vendor);

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle);
            return new ServiceResponseBase(getExtrasResponse, true);
        }
    }
}


