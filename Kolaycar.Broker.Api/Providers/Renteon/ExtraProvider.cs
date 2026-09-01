using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Renteon;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Renteon
{
    public class ExtraProvider : IExtraProvider
    {
        RestManager _restManager;
        AuthProvider _authProvider { get; set; }

        IVehicleProvider _vehicleProvider { get; set; }

        public ExtraProvider(string apiBaseUrl)
        {
            _restManager = new RestManager(apiBaseUrl);
            _authProvider = new AuthProvider();
        }

        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var vendorName = vendor.ApiClientId.Split("-")[1];
            var result = await _restManager.GetAsync<RenteonResponseBase.RenteonLocationResponseBase>(
               requestPath: $"/api/setup/provider/{vendorName}",
               headers: _authProvider.GetBasicAuth(vendor)
               );

            if (result != null)
            {
                return new ServiceResponseBase
                {
                    Data = result.Services.Map(vendor),
                    Success = true
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Renteon servisine ulaşılamadı"
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
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == additionalInformation.ReservationToken.VehicleCode);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            var extras = selectedVehicle.Extras;

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle, vehicles);
            return new ServiceResponseBase(getExtrasResponse, true);
        }
    }
}
