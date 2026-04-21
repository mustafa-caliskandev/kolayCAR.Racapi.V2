using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Wishcar;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WishcarProvider = KolayCAR.Broker.API.Providers.Wishcar;

namespace KolayCAR.Broker.API.Providers.Wishcar
{
    public class ExtraProvider : IExtraProvider
    {
        IVehicleProvider vehicleProvider;
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        public ExtraProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
            AuthProvider = new AuthProvider(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var auth = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);
            if (auth != null && !string.IsNullOrEmpty(auth.access_token))
            {
                var result = await RestManager.GetAsync<WishcarRequestBase.ExtraRequest, List<WishcarResponseBase.ExtraResponse>>(
                    requestPath: vendor.APIBaseUrl + $"api/Extras/List",
                    entity: GetExtrasRequestBodyEntity(),
                    headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token));

                if (result != null)
                {
                    return new ServiceResponseBase
                    {
                        Success = result != null,
                        Data = result.Map()
                    };
                }
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Kimlik doğrulama işlemi başarısız!"
            };
        }

        private WishcarRequestBase.ExtraRequest GetExtrasRequestBodyEntity() =>
       new WishcarRequestBase.ExtraRequest
       {
           days = 1
       };
        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            vehicleProvider = new VehicleProvider(vendor, false);
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var extras = new List<Extra>();

            var auth = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);

            if (!string.IsNullOrEmpty(auth?.access_token))
            {
                var result = await RestManager.GetAsync<WishcarRequestBase.ExtraRequest, List<WishcarResponseBase.ExtraResponse>>(
                    requestPath: vendor.APIBaseUrl + $"api/Extras/List",
                    entity: GetExtrasAvaibilityRequestBodyEntity(reservationToken.RentalDuration),
                    headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token));

                if (result != null)
                {
                    extras = result.Map();
                    CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);
                }

            }
            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);
            var getVehiclesResponse = await vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == reservationToken.VehicleCode);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle);
            return new ServiceResponseBase(getExtrasResponse, true);
        }

        private WishcarRequestBase.ExtraRequest GetExtrasAvaibilityRequestBodyEntity(int daysCount) =>
     new WishcarRequestBase.ExtraRequest
     {
         days = daysCount,
     };
    }
}
