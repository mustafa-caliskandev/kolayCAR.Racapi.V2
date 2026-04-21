using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Beto;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.BetoRequestBase;
using static KolayCAR.Broker.Domain.Models.Response.BetoResponseBase;

namespace KolayCAR.Broker.API.Providers.Beto
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
                var result = await RestManager.GetAsync<ExtraRequest, List<ExtraResponse>>(
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

        private ExtraRequest GetExtrasRequestBodyEntity() =>
       new ExtraRequest
       {
           days = 1
       };
        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var auth = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);
            if (!string.IsNullOrEmpty(auth?.access_token))
            {
                var result = await RestManager.PostAsyncWithUrlEncoded<ExtraRequest, List<ExtraResponse>>(
                    requestPath: vendor.APIBaseUrl + $"api/Extras/List",
                    entity: GetExtrasAvaibilityRequestBodyEntity2(reservationToken.RentalDuration, getExtrasRequest),
                    headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token));

                if (result != null)
                {
                    vehicleProvider = new VehicleProvider(vendor, false);
                    var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

                    var getVehiclesResponse = await vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

                    var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
                    var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == reservationToken.VehicleCode);

                    if (selectedVehicle == null)
                        return new ServiceResponseBase(null, false);

                    var extras = result?.Map();

                    CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
                    CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

                    var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle);
                    return new ServiceResponseBase(getExtrasResponse, true);
                }
            }

            return new ServiceResponseBase(null, false, "Kimlik doğrulama işlemi başarısız!");
        }

        private List<KeyValuePair<string, string>> GetExtrasAvaibilityRequestBodyEntity2(int daysCount, GetExtrasRequest getExtrasRequest)
        {
            var currency = getExtrasRequest.CurrencyCode switch
            {
                "EUR" => "EURO",
                "USD" => "USD",
                _ => "TL"
            };
            return new List<KeyValuePair<string, string>>
                    {
                        new("days", daysCount.ToString()),
                        new("currency", currency)
                    };
        }
    }
}
