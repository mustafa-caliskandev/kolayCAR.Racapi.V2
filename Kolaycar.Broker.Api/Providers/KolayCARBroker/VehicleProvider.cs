using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.KolayCARBroker;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.KolayCARBroker
{
    public class VehicleProvider : IVehicleProvider
    {
        HttpManager HttpManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        ICacheService _cacheService { get; set; }

        public VehicleProvider(Vendor vendor, ICacheService cacheService, bool disableTimeout)
        {
            HttpManager = new HttpManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            AuthProvider = new AuthProvider(vendor.APIBaseUrl);
            _cacheService = cacheService;
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            try
            {
                var auth = await _cacheService.GetOrCreateAsync($"kolayCarBroker{vendor.VendorName}Token", () => AuthProvider.GetJWT(vendor.ApiKey, vendor.ApiPassword, EncryptionHelper.Encrypt(vendor.ApiPassword)), TimeSpan.FromMinutes(30));

                if (auth?.Data != null)
                {
                    var user = auth.Data as User;
                    var result = await HttpManager.GetAsync<List<Vehicle>>(
                        requestPath: "vehicles",
                        parameters: CreateBrokerGetVehiclesRequestParameters(getVehiclesRequest, baseVendorRequestCurrencyType, additionalInformation),
                        headers: AuthProvider.CreateAuthHeader(user.Token));
                    if (result?.Data?.Count > 0)
                    {
                        //var apiVehicleList = JsonConvert.DeserializeObject<List<Vehicle>>(JsonConvert.SerializeObject(result.Data));
                        var apiVehicleList = VehicleHelper.SelectCheapestByGroup(JsonConvert.DeserializeObject<List<Vehicle>>(JsonConvert.SerializeObject(result.Data)), v => v.VehicleId,
                          v => v.DailyPrice.ToFloatNullSafe());

                        var mappedVehicleList = VehicleHelper.SelectCheapestByGroup(JsonConvert.DeserializeObject<List<Vehicle>>(JsonConvert.SerializeObject(result.Data)), v => v.VehicleId,
                          v => v.DailyPrice.ToFloatNullSafe());
                        var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                        if (!vendor.UseBrokerConfigurations)
                        {
                            mappedVehicleList = mappedVehicleList.Map(additionalInformation);
                            if (vendor.VehicleMappingActive)
                            {
                                mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, useBaseVehiclePropsFromVendorAPI: true, vendor: vendor);
                                apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.VehicleId.ToStringNullSafe()));
                            }
                            CalculationHelper.SetVehiclesPrices(mappedVehicleList, vendor, exchangeRates, requestCurrencyType, baseVendorRequestCurrencyType);
                            VehicleHelper.SetVehiclesProperties(mappedVehicleList, vendor, additionalInformation.Agency, exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, profitMarkups);
                        }
                        else
                        {
                            mappedVehicleList.ForEach(x =>
                            {
                                x.ServiceCharge = CalculationHelper.CurrencyExchange(exchangeRates, vendor, vendor.ServiceCharge, vendor.ServiceChargeCurrencyType, requestCurrencyType);
                            });
                            mappedVehicleList = mappedVehicleList.Select(x => CalculationHelper.CalculateFinalVehiclePrices(VendorHelper.SetVendorProfitMarkup(vendor, subVendors.Where(s => s.SubVendorId == x.VendorId).FirstOrDefault()), x, additionalInformation.Agency, requestCurrencyType, profitMarkups, exchangeRates: exchangeRates)).ToList();
                        }
                        if (mappedVehicleList.Any(vehicle => Math.Abs(vehicle.RentalDuration - additionalInformation.RentalDuration) > 1))
                            return new ServiceResponseBase(null, false, "Yanlış gün sayısı");

                        var pickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime);
                        var returnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime);
                        var languageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>();

                        foreach (var vehicle in apiVehicleList.Select((value, index) => new { value, index }))
                        {
                            //var tempMappedVehicleList = !vendor.UseBrokerConfigurations ? mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.VehicleId.ToStringNullSafe()).ToList() : mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.VehicleCode.ToStringNullSafe() && x.VehicleId == vehicle.value.VehicleId && x.VendorId == vehicle.value.VendorId).ToList();
                            var tempMappedVehicleList = !vendor.UseBrokerConfigurations ? mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.VehicleId.ToStringNullSafe()).ToList() : mappedVehicleList.Where(x => x.ReservationToken == vehicle.value.ReservationToken).ToList();
                            for (int i = 0; i < tempMappedVehicleList.Count; i++)
                            {
                                var mappedVehicle = tempMappedVehicleList[i];

                                var reservationToken = new ReservationToken
                                {
                                    AgencyId = additionalInformation.Agency.AgencyId,
                                    VendorId = vendor.VendorId,
                                    APIVendorId = mappedVehicle.VendorId == 0 ? vendor.VendorId : mappedVehicle.VendorId,
                                    APIVendorName = mappedVehicle.VendorName,
                                    APIVendorPhone = mappedVehicle.VendorPhone,
                                    APIVendorEmail = mappedVehicle.VendorEmail,
                                    APIVendorLogo = mappedVehicle.VendorLogo,
                                    VehicleId = vehicle.value.VehicleId,
                                    VehicleCode = mappedVehicle.VehicleCode,
                                    APIPickupLocationId = additionalInformation.APIPickupLocationId,
                                    APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                                    APIReturnLocationId = additionalInformation.APIReturnLocationId,
                                    APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                                    CurrencyType = requestCurrencyType,
                                    RentalDuration = mappedVehicle.RentalDuration,
                                    DailyPrice = mappedVehicle.DailyPrice,
                                    OneWayFee = mappedVehicle.OneWayFee,
                                    DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                                    APIDailyPrice = vehicle.value.DailyPrice,
                                    APIDailyPricePayNow = vehicle.value.DailyPricePayNow,
                                    APIOneWayFee = vehicle.value.OneWayFee,
                                    APIReferenceCode = vehicle.value.ReservationToken,
                                    DepositPrice = vehicle.value.DepositPrice,
                                    VendorMinimumDriverAge = vehicle.value.VendorMinimumDriverAge ?? 0,
                                    VendorMinimumDrivingLicenseAge = vehicle.value.VendorMinimumDrivingLicenseAge ?? 0,
                                    ServiceCharge = mappedVehicle.ServiceCharge,
                                    FuelType = mappedVehicle.FuelType,
                                    TransmissionType = mappedVehicle.TransmissionType,
                                    IsOffice = vehicle.value.IsOffice,
                                    IsAirport = vehicle.value.IsAirport,
                                    DepositCreditCardRequired = mappedVehicle.DepositCreditCardRequired,
                                    PickupLocationId = getVehiclesRequest.PickupLocationId,
                                    ReturnLocationId = getVehiclesRequest.ReturnLocationId,
                                    LanguageType = languageType,
                                    PickupDateTime = pickupDateTime,
                                    ReturnDateTime = returnDateTime,
                                    VehicleName = mappedVehicle.VehicleName,
                                    VehicleImageUrl = mappedVehicle.VehicleImages.Count > 0 ? mappedVehicle.VehicleImages[0].Url : string.Empty,
                                    BaseVendorRequestCurrencyType = baseVendorRequestCurrencyType,
                                    VehicleCategoryType = mappedVehicle.VehicleCategoryType,
                                    ApiVendorType = mappedVehicle.VendorType,
                                    APIFullCredit = mappedVehicle.FullCredit,
                                    SpecialProfitApplied = mappedVehicle.SpecialProfitApplied,
                                    BaggageQuantityType = mappedVehicle.BaggageQuantityType,
                                    PassangerQuantityType = mappedVehicle.PassangerQuantityType,
                                    TotalKmLimit = mappedVehicle.TotalKMLimit ?? 0,
                                    VehicleType = mappedVehicle.VehicleType,
                                    VendorFlightPassRequired = vendor.FlightNumberRequired ?? false,
                                    FullCredit = mappedVehicle.FullCredit,
                                    SippCode = mappedVehicle.SippCode,
                                    BaseBaseVendorRequestCurrencyType = vehicle.value.BaseVendorCurrencyTypes,
                                    APIDeliveryTypeId = mappedVehicle.ApiDeliveryTypeId
                                };

                                mappedVehicle.ReservationToken = reservationToken.ToJson();

                                if (additionalInformation.Agency.SpecialParameters)
                                {
                                    mappedVehicle.SpecialVendorId = vendor.VendorId.ToString();
                                    mappedVehicle.SpecialVendorName = vendor.VendorName;
                                    mappedVehicle.SpecialVendorLogo = vendor.Logo;
                                }
                                //mappedVehicleList[vehicle.index] = mappedVehicle;
                            }
                        }

                        if (result.Success)
                        {

                            //var newFilterList = new List<Vehicle>();

                            mappedVehicleList.RemoveAll(x => x.ReservationToken.Length < 100);

                            //newFilterList = mappedVehicleList;
                            //foreach (var item in mappedVehicleList)
                            //{
                            //    if (item.ReservationToken.Length > 100)
                            //    {
                            //        newFilterList.Add(item);
                            //    }
                            //}

                            return new ServiceResponseBase
                            {
                                Success = result.Success,
                                Message = result.Message,
                                ServiceMessage = result.Message,
                                Data = mappedVehicleList
                            };
                        }
                    }
                    return new ServiceResponseBase
                    {
                        Success = auth.Success,
                        Message = "Müsait araç bulunamadı!",
                        ServiceMessage = auth.Message
                    };

                }
                Serilog.Log.Error("{@kolayCarBrokerTokenError}", $"kolaycarBroker {vendor.VendorName} token alınamadı");
                return new ServiceResponseBase
                {
                    Success = auth.Success,
                    Message = "Kimlik doğrulama işlemi başarısız!",
                    ServiceMessage = auth.Message
                };
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@kolayCarBrokerError}", $"{ex.ToJson()}");
                return new ServiceResponseBase();
            }

        }

        private Dictionary<string, object> CreateBrokerGetVehiclesRequestParameters(GetVehiclesRequest getVehiclesRequest, CurrencyTypes baseVendorRequestCurrencyType, ResponseReservationStepsAdditionalInformation additionalInformation)
        {
            return new Dictionary<string, object>()
            {
                { "languageCode",  getVehiclesRequest.LanguageCode},
                { "currencyCode",  baseVendorRequestCurrencyType.ToString()},
                { "pickupLocationId",  additionalInformation.APIPickupLocationCode},
                { "returnLocationId",  additionalInformation.APIReturnLocationCode},
                { "pickupDate",  getVehiclesRequest.PickupDate},
                { "returnDate",  getVehiclesRequest.ReturnDate},
                { "pickupTime",  getVehiclesRequest.PickupTime},
                { "returnTime",  getVehiclesRequest.ReturnTime},
                { "disableTimeout", getVehiclesRequest.DisableTimeout}
            };
        }
        private Dictionary<string, object> CreateBrokerGetVehiclesListRequestParameters(string language)
        {
            return new Dictionary<string, object>()
            {
                { "languageCode",  language}
            };
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var auth = await AuthProvider.GetJWT(vendor.ApiKey, vendor.ApiPassword, EncryptionHelper.Encrypt(vendor.ApiPassword));

            if (auth != null)
            {
                var user = auth.Data as User;

                var result = await HttpManager.GetAsync<List<Vehicle>>(
                    requestPath: "vehicles/list",
                    parameters: CreateBrokerGetVehiclesListRequestParameters("TR"),
                    headers: AuthProvider.CreateAuthHeader(user.Token));

                if (result?.Data?.Count > 0)
                {
                    return new ServiceResponseBase(result.Data.Map(), result.Success, result.Message, result.Message);
                }
            }
            return new ServiceResponseBase(null, auth.Success, "Kimlik doğrulama işlemi başarısız!", auth.Message);
        }
    }
}
