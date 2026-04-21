using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers._5S;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.BesSRequestBase;

namespace KolayCAR.Broker.API.Providers._5S
{
    public class ExtraProvider : IExtraProvider
    {
        RestManager RestManager { get; set; }
        //Configuration Configuration { get; set; }
        IVehicleProvider vehicleProvider;
        public ExtraProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
            //Configuration = new Configuration();
        }
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var result = await RestManager.GetAsync<BesSResponseBase.ExtraResponse>(
                requestPath: "extra",
                headers: Configuration.CreateHeaderWithAuth(vendor.ApiKey)
                );

            if (result != null && result.data != null && result.data.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Data = result.Map(),
                    Success = true
                };
            }

            return new ServiceResponseBase
            {
                Data = null,
                Success = false
            };
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            vehicleProvider = new VehicleProvider(vendor, false);
            var token = additionalInformation.ReservationToken;
            var currencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var languageType = getExtrasRequest.LanguageCode.ToEnum<LanguageTypes>();

            var extraSearchResponse = await RestManager.PostAsync<AvailableVehicleRequest, List<BesSResponseBase.ExtraSearchResponse>>(
                requestPath: "extservice/extra_search",
                headers: Configuration.CreateHeaderWithAuth(vendor.ApiKey),
                entity: GetExtraSearchRequestBody(additionalInformation, token)
            );
            var extras = extraSearchResponse?.Map();

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);
            var vehicleResponse = await vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, token.BaseVendorRequestCurrencyType);

            var vehicles = vehicleResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == token.VehicleCode.ToString());

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, currencyType, token, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, currencyType, addProfitMarkup, getAPIPrices, token.BaseVendorRequestCurrencyType);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle);
            return new ServiceResponseBase(getExtrasResponse, true);
        }

        private AvailableVehicleRequest GetExtraSearchRequestBody(ResponseReservationStepsAdditionalInformation additionalInformation, ReservationToken token)
        {
            return new AvailableVehicleRequest
            {
                begin_date = additionalInformation.PickupDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                end_date = additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                begin_location = additionalInformation.APIPickupLocationCode,
                end_location = additionalInformation.APIReturnLocationCode,
                currency = GetCurrency(token.BaseVendorRequestCurrencyType)
            };
        }
        private string GetCurrency(CurrencyTypes baseVendorRequestCurrencyType)
        {
            return baseVendorRequestCurrencyType switch
            {
                CurrencyTypes.TRY => "TRY",
                CurrencyTypes.USD => "USD",
                CurrencyTypes.EUR => "EUR",
                CurrencyTypes.GBP => "GBP",
                CurrencyTypes.CHF => "CHF",
                _ => ""
            };
        }
        //private static Dictionary<string, object> GetExtraSearchRequestBody(ResponseReservationStepsAdditionalInformation additionalInformation)
        //{
        //    return new Dictionary<string, object>
        //    {
        //        { "begin_date", additionalInformation.PickupDateTime.ToString("yyyy-MM-dd HH:mm:ss") },
        //        { "end_date", additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd HH:mm:ss") },
        //        { "currency", additionalInformation.CurrencyCode}
        //    };
        //}
    }
}
