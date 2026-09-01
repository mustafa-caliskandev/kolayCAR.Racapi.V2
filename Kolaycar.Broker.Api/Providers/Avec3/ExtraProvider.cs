using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Avec3;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.Avec3ResponseBase;

namespace KolayCAR.Broker.API.Providers.Avec3
{
    public class ExtraProvider : IExtraProvider
    {
        VehicleProvider _vehicleProvider;
        HttpManager _httpManager;
        AuthProvider _authProvider;
        public ExtraProvider()
        {

        }
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            _authProvider = new AuthProvider(vendor.APIBaseUrl);
            _httpManager = new HttpManager(vendor.APIBaseUrl);
            var user = await _authProvider.GetTokenAsync(vendor.ApiKey, vendor.ApiPassword);
            var result = await _httpManager.GetAsync2<List<Avec3ResponseBase.ExtraResponseAvec>>
                (
                requestPath: $"/branch_base/vehicle/addons",
                headers: _authProvider.CreateAuthHeader(user.Data.access_token)
                );
            return new ServiceResponseBase()
            {
                Data = result.Data.Map(),
                Success = result.Success
            };
            //return new ServiceResponseBase { Data = null, Success = true };
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Domain.Models.Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            _vehicleProvider = new VehicleProvider(vendor, false);
            _authProvider = new AuthProvider(vendor.APIBaseUrl);
            _httpManager = new HttpManager(vendor.APIBaseUrl);
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
         
            var user = await _authProvider.GetTokenAsync(vendor.ApiKey, vendor.ApiPassword);

            if(user?.Data?.access_token == "")
                return new ServiceResponseBase(null, false);
            
            var result = await _httpManager.GetAsync2<AvailableExtraResponseAvec>
                (
                requestPath: $"/branch_base/booking/{reservationToken.APIReferenceCode}/addons",
                headers: _authProvider.CreateAuthHeader(user.Data.access_token)
                );

            if (result?.Data?.data == null)
                return new ServiceResponseBase(null, false);

            var extras = result.Data.data.Map();

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await _vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Domain.Models.Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleId == additionalInformation.VehicleId);

            if (selectedVehicle != null)
                return new ServiceResponseBase(null, false);

            selectedVehicle.Extras = extras;

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle, vehicles);
            return new ServiceResponseBase(getExtrasResponse, true);
        }
    }
}
