using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Elitcar;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElitcarProvider = KolayCAR.Broker.API.Providers.Elitcar;

namespace KolayCAR.Broker.API.Providers.Elitcar
{
    public class ExtraProvider : IExtraProvider
    {
        IVehicleProvider vehicleProvider;
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }

        public ExtraProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
            AuthProvider = new AuthProvider();
        }

        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var result = await RestManager.GetAsync<ElitcarRequestBase.MasterListRequest, ElitcarResponseBase>(
                requestPath: vendor.APIBaseUrl + $"extras",
                entity: GetMasterRequestBodyEntity(vendor),
                headers: AuthProvider.CreateHeaderWithContentType());

            if (result != null && result.extras != null && result.count > 0 && result.extras.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = result != null,
                    Data = result.extras.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Kimlik doğrulama işlemi başarısız!",
            };
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            vehicleProvider = new VehicleProvider(vendor, false);
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var requestLanguageType = getExtrasRequest.LanguageCode.ToEnum<LanguageTypes>();

            var result = await RestManager.GetAsync<ElitcarRequestBase.AvailabilityRequest, ElitcarResponseBase>(
                requestPath: vendor.APIBaseUrl + $"query",
                entity: GetAdditionalProductBodyEntity(additionalInformation, vendor),
                headers: AuthProvider.CreateHeaderWithContentType());

            var extras = new List<Extra>();

            if (result?.services?.count > 0)
            {
                extras = result.services.list?.Map();
                CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);
            }

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == reservationToken.VehicleCode);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle, vehicles);
            return new ServiceResponseBase(getExtrasResponse, true);
        }

        private ElitcarRequestBase.MasterListRequest GetMasterRequestBodyEntity(Vendor vendor) =>
       new ElitcarRequestBase.MasterListRequest
       {
           locale = "tr",
           username = vendor.ApiKey,
           password = vendor.ApiPassword
       };

        private ElitcarRequestBase.AvailabilityRequest GetAdditionalProductBodyEntity(ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
   new ElitcarRequestBase.AvailabilityRequest
   {
       username = vendor.ApiKey,
       password = vendor.ApiPassword,
       pick_id = additionalInformation.APIPickupLocationCode.ToIntNullSafe(),
       pick_date_time = additionalInformation.PickupDateTime.ToString("dd.MM.yyyy HH:mm"),
       drop_id = additionalInformation.APIReturnLocationCode.ToIntNullSafe(),
       drop_date_time = additionalInformation.ReturnDateTime.ToString("dd.MM.yyyy HH:mm"),
       country_code = "tr"
   };

    }
}
