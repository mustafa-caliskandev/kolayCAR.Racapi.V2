using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Cizgi;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CizgiProvider = KolayCAR.Broker.API.Providers.Cizgi;

namespace KolayCAR.Broker.API.Providers.Cizgi
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
            if (!string.IsNullOrEmpty(vendor.SecretKey))// TODO: silinecek
            {
                Serilog.Log
                    .ForContext("Token", auth?.access_token.ToStringNullSafe())
                    .Error("{@CizgiAuthLog}", auth);
            }
            if (auth != null && !string.IsNullOrEmpty(auth.access_token))
            {
                var result = await RestManager.GetAsync<CizgiResponseBase.ExtraResponse>(
                    requestPath: "reservation/packages",
                    headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token));

                if (!string.IsNullOrEmpty(vendor.SecretKey))
                {
                    Serilog.Log
                        .ForContext("Fleet", result?.ToStringNullSafe())
                        .Error("{@CizgiResultLog}", result);
                }

                if (result != null && result.status == 1 && result.packages != null && result.packages.Count > 0)
                {
                    return new ServiceResponseBase
                    {
                        Success = result != null,
                        Data = result.packages.Map()
                    };
                }
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Kimlik doğrulama işlemi başarısız!"
            };
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {     
            vehicleProvider = new VehicleProvider(vendor, false);
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var requestLanguageType = getExtrasRequest.LanguageCode.ToEnum<LanguageTypes>();
       
            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

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

        private CizgiRequestBase.AvailabilityRequest GetAdditionalProductBodyEntity(ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
     new CizgiRequestBase.AvailabilityRequest
     {

         pickup_location = additionalInformation.APIPickupLocationCode.ToIntNullSafe(),
         pickup_date = additionalInformation.PickupDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
         dropoff_location = additionalInformation.APIReturnLocationCode.ToIntNullSafe(),
         dropoff_date = additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
         rate_codes = null
     };
    }
}
