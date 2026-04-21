using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.FiloNova;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.FiloNova
{
    public class ExtraProvider : IExtraProvider
    {
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        private readonly IMemoryCache _memoryCache;

        public ExtraProvider(string apiBaseUrl, IMemoryCache memoryCache)
        {
            RestManager = new RestManager(apiBaseUrl);
            AuthProvider = new AuthProvider();
            _memoryCache = memoryCache;
        }

        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var result = new FiloNovaResponseBase();

            if (vendor.VendorName == "Rent Go")
                result = JsonHelper.ReadFromJsonFile<FiloNovaResponseBase>(@"Docs\RentGo\RentGoAdditionalProducts.json");
            else
                await RestManager.PostAsync<FiloNovaRequestBase.MasterRequest, FiloNovaResponseBase>(
                requestPath: $"getmasterdata",
                entity: GetMasterRequestBodyEntity(vendor),
                headers: AuthProvider.CreateAuthHeaderWithContentType(vendor));

            if (result?.additionalProducts?.Count > 0)
                return new ServiceResponseBase(result.additionalProducts.Map(), result != null);

            return new ServiceResponseBase($"{vendor.VendorName} servisinden bilgi alınamadı!", false);
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            var vehicleProvider = new VehicleProvider(vendor, _memoryCache, false);
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var requestLanguageType = getExtrasRequest.LanguageCode.ToEnum<LanguageTypes>();

            var result = await RestManager.PostAsync<FiloNovaRequestBase.AdditionalProductQueryParameters, FiloNovaResponseBase>(
                requestPath: $"getAdditionalProducts",
                entity: GetAdditionalProductBodyEntity(additionalInformation, vendor, reservationToken),
                headers: AuthProvider.CreateAuthHeaderWithContentType(vendor));            

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == reservationToken.VehicleCode);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            var extras = result?.additionalProducts?.Map();

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);
       
            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle);
            return new ServiceResponseBase(getExtrasResponse, true);
        }

        private FiloNovaRequestBase.AdditionalProductQueryParameters GetAdditionalProductBodyEntity(ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, ReservationToken reservationToken) =>
        new FiloNovaRequestBase.AdditionalProductQueryParameters
        {
            queryParameters = new FiloNovaRequestBase.AvailabilityQueryParameters
            {
                pickupBranchId = additionalInformation.APIPickupLocationCode,
                dropoffBranchId = additionalInformation.APIReturnLocationCode,
                pickupDateTime = additionalInformation.PickupDateTime.ToString("yyyy-MM-ddTHH:mm:ss") + "+03:00",
                dropoffDateTime = additionalInformation.ReturnDateTime.ToString("yyyy-MM-ddTHH:mm:ss") + "+03:00"
            },
            groupCodeId = reservationToken.VehicleCode,
            brokerCode = vendor.ApiClientId,
            langId = vendor.SecretKey.ToIntNullSafe()
        };

        private FiloNovaRequestBase.MasterRequest GetMasterRequestBodyEntity(Vendor vendor) =>
        new FiloNovaRequestBase.MasterRequest
        {
            brokerCode = vendor.ApiClientId,
            langId = vendor.SecretKey.ToIntNullSafe()
        };
    }
}
