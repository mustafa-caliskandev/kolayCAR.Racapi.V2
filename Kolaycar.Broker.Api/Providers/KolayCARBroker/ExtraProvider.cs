using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.KolayCARBroker;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.KolayCARBroker
{
    public class ExtraProvider : IExtraProvider
    {
        HttpManager HttpManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        ICacheService _cacheService { get; set; }
        IConfiguration _configuration { get; set; }
        public ExtraProvider(string apiBaseUrl, ICacheService cacheService, IConfiguration configuration)
        {
            HttpManager = new HttpManager(apiBaseUrl);
            AuthProvider = new AuthProvider(apiBaseUrl);
            _cacheService = cacheService;
            _configuration = configuration;
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            var brokerName = _configuration["AppSettings:BrokerName"].ToStringNullSafe();

            var auth = await _cacheService.GetOrCreateAsync($"kolayCarBroker{vendor.VendorName}Token", () => AuthProvider.GetJWT(vendor.ApiKey, vendor.ApiPassword, EncryptionHelper.Encrypt(vendor.ApiPassword))
            , TimeSpan.FromMinutes(30));

            if (auth != null)
            {
                var user = auth.Data as User;
                var reservationToken = additionalInformation.ReservationToken;

                var result = await HttpManager.GetAsync<GetExtrasResponse>(
                    requestPath: "extras",
                    parameters: CreateBrokerGetExtrasRequestParameters(getExtrasRequest, reservationToken, additionalInformation),
                    headers: AuthProvider.CreateAuthHeader(user.Token));

                if ((bool)result?.Success)
                {
                    var extrasResponseData = result.Data as GetExtrasResponse;
                    var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                    extrasResponseData.Extras?.RemoveAll(e => e.ExtraType == AdditionalProductTypes.Premium && !e.VendorExtraExists);
                    var getExtrasResponse = new GetExtrasResponse
                    {
                        Extras = !vendor.UseBrokerConfigurations ? extrasResponseData.Extras.Map() : extrasResponseData.Extras,
                        Vehicle = CalculationHelper.CalculateFinalVehiclePrices(vendor, !vendor.UseBrokerConfigurations ? extrasResponseData.Vehicle.Map(additionalInformation) : extrasResponseData.Vehicle, additionalInformation.Agency),
                        AlternativeVehicles = !vendor.UseBrokerConfigurations
                            ? extrasResponseData.AlternativeVehicles.Map(additionalInformation)
                            : extrasResponseData.AlternativeVehicles
                    };

                    CalculationHelper.SetVehiclePrices(getExtrasResponse.Vehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration, useVendorProps: !vendor.UseBrokerConfigurations);
                    CalculationHelper.SetExtraPrices(getExtrasResponse.Extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);
                    getExtrasResponse.Extras = ReservationHelper.RemoveZeroPriceExtras(getExtrasResponse.Extras);
                    if (brokerName == "Airtuerk")
                    {
                        if (getExtrasResponse?.Extras != null)
                            foreach (var extra in getExtrasResponse.Extras)
                            {
                                if (extra.ExtraType == AdditionalProductTypes.Premium)
                                {
                                    extra.Price = extra.DefaultPrice;
                                }
                                extra.Icon = extra.Icon != null ? extra.Icon.Replace("www.netrentacar.de", "rentacar.airtuerk.net") : null;
                            }
                    }

                    if (vendor.VehicleMappingActive)
                    {
                        getExtrasResponse.Vehicle = VehicleHelper.MapLocalVehicle(getExtrasResponse.Vehicle, localVehicles.Where(x => x.VehicleCode == getExtrasResponse.Vehicle.VehicleId.ToStringNullSafe()).FirstOrDefault(), useBaseVehiclePropsFromVendorAPI: true);
                    }

                    return new ServiceResponseBase
                    {
                        Success = result.Success,
                        Message = result.Message,
                        ServiceMessage = result.Message,
                        Data = getExtrasResponse
                    };
                }

                return new ServiceResponseBase
                {
                    Success = false,
                    Message = result.Message,
                    ServiceMessage = result.Message
                };
            }

            return new ServiceResponseBase
            {
                Success = auth.Success,
                Message = "Kimlik doğrulama işlemi başarısız!",
                ServiceMessage = auth.Message
            };
        }

        private Dictionary<string, object> CreateBrokerGetExtrasRequestParameters(GetExtrasRequest getExtrasRequest, ReservationToken reservationToken, ResponseReservationStepsAdditionalInformation additionalInformation)
        {
            return new Dictionary<string, object>()
            {
                { "languageCode",  getExtrasRequest.LanguageCode},
                { "currencyCode",  reservationToken.BaseVendorRequestCurrencyType.ToString()},
                { "pickupLocationId",  additionalInformation.APIPickupLocationCode},
                { "returnLocationId",  additionalInformation.APIReturnLocationCode},
                { "pickupDate", getExtrasRequest.PickupDate},
                { "returnDate", getExtrasRequest.ReturnDate},
                { "pickupTime", getExtrasRequest.PickupTime},
                { "returnTime", getExtrasRequest.ReturnTime},
                { "reservationToken",  reservationToken.APIReferenceCode}
            };
        }

        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var auth = await AuthProvider.GetJWT(vendor.ApiKey, vendor.ApiPassword, EncryptionHelper.Encrypt(vendor.ApiPassword));

            if (auth != null)
            {
                var user = auth.Data as User;

                var result = await HttpManager.GetAsync<List<Extra>>(
                    requestPath: "extras/list",
                    parameters: CreateBrokerGetExtraListRequestParameters(currencyType.ToString(), languageType.ToString(), rentalDuration),
                    headers: AuthProvider.CreateAuthHeader(user.Token));

                if (result != null && result.Data != null && result.Data.Count > 0)
                    return new ServiceResponseBase
                    {
                        Success = result.Success,
                        Message = result.Message,
                        ServiceMessage = result.Message,
                        Data = result.Data.Map() as List<Extra>
                    };
                else
                    return new ServiceResponseBase
                    {
                        Success = false,
                        Message = "Broker servisinden veri alınamadı!",
                        Data = null
                    };
            }

            return new ServiceResponseBase
            {
                Success = auth.Success,
                Message = "Kimlik doğrulama işlemi başarısız!",
                ServiceMessage = auth.Message
            };
        }

        private Dictionary<string, object> CreateBrokerGetExtraListRequestParameters(string currencyCode, string languageCode, int rentalDuration)
        {
            rentalDuration = rentalDuration <= 0 ? 1 : rentalDuration;

            return new Dictionary<string, object>()
            {
                { "currencyCode",  currencyCode},
                { "languageCode",  languageCode},
                { "rentalDuration",  rentalDuration}
            };
        }
    }
}
