using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.KolayCAR;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Resws.ServiceSoapClient;

namespace KolayCAR.Broker.API.Providers.KolayCAR
{
    public class ExtraProvider : IExtraProvider
    {
        private readonly Resws.ServiceSoapClient _kolayCARService;

        public ExtraProvider(Vendor vendor)
        {
            _kolayCARService = new Resws.ServiceSoapClient(EndpointConfiguration.ServiceSoap, Helpers.KolayCAR.ReservationHelper.GetSoapRemoteAddress(vendor.APIBaseUrl));
        }

        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            rentalDuration = rentalDuration <= 0 ? 1 : rentalDuration;
            var getExtrasAllResult = await _kolayCARService.GET_EXTRAS_ALLAsync(
                                vendor.ApiKey,
                                vendor.ApiPassword,
                                languageType.ToString(),
                                currencyType.ToString(),
                                0,
                                0,
                                DateTime.Now.AddDays(1).ToShortDateString(),
                                DateTime.Now.AddDays(rentalDuration + 1).ToShortDateString(),
                                "10:00",
                                "10:00");

            var getExtrasAllResponseObject = JsonConvert.DeserializeObject<KolayCARResponseBase>(getExtrasAllResult.Body.GET_EXTRAS_ALLResult);

            return new ServiceResponseBase
            {
                Success = getExtrasAllResponseObject.RETURNCODE == 0,
                ServiceCode = getExtrasAllResponseObject.RETURNCODE.ToString(),
                ServiceMessage = getExtrasAllResponseObject.MESSAGE,
                Data = getExtrasAllResponseObject.EXTRAS.Map()
            };
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            var reservationToken = additionalInformation.ReservationToken;
            int vehicleId = reservationToken.VehicleCode.Split('-')[0].ToIntNullSafe();
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();

            var getExtrasResult = await _kolayCARService.GET_VEHICLE_EXTRAS1Async(
                vendor.ApiKey,
                vendor.ApiPassword,
                getExtrasRequest.LanguageCode,
                reservationToken.BaseVendorRequestCurrencyType.ToString(),
                Convert.ToInt32(additionalInformation.APIPickupLocationCode),
                Convert.ToInt32(additionalInformation.APIReturnLocationCode),
                getExtrasRequest.PickupDate,
                getExtrasRequest.ReturnDate,
                getExtrasRequest.PickupTime,
                getExtrasRequest.ReturnTime,
                additionalInformation.APIVendorId,
                vehicleId,
                getExtrasRequest.UserToken,
                getExtrasRequest.CouponCode,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);

            var getExtrasResponseObject = JsonConvert.DeserializeObject<KolayCARResponseBase>(getExtrasResult.GET_VEHICLE_EXTRAS_V2Result);
            KOLAYCARSETTINGS getSettingsResponse = null;
            if (getExtrasResponseObject.RETURNCODE == 0)
            {
                var getExtrasResponse = new GetExtrasResponse
                {
                    Extras = getExtrasResponseObject.EXTRAS.Map(),
                    Vehicle = CalculationHelper.CalculateFinalVehiclePrices(vendor, getExtrasResponseObject.VEHICLES[0].Map(additionalInformation, vendor), additionalInformation.Agency)
                };

                CalculationHelper.SetVehiclePrices(getExtrasResponse.Vehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
                CalculationHelper.SetExtraPrices(getExtrasResponse.Extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

                getExtrasResponse.Extras = ReservationHelper.RemoveZeroPriceExtras(getExtrasResponse.Extras);
                var sourceCurrenyType = (bool)vendor.UseLocalDeposit ? vendor.CurrencyType : reservationToken.BaseVendorRequestCurrencyType;
                if (vendor.VehicleMappingActive)
                {
                    getExtrasResponse.Vehicle = VehicleHelper.MapLocalVehicle(getExtrasResponse.Vehicle, localVehicles.Where(x => x.VehicleCode == getExtrasResponse.Vehicle.VehicleCode).FirstOrDefault(), sourceCurrenyType: sourceCurrenyType, targetCurrenyType: requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, exchangeRates: exchangeRates);
                }

                if (vendor.CreditType == CreditType.FullCredit && additionalInformation.Agency.FullCreditPermission)
                {
                    var getSettingsResult = await _kolayCARService.GET_SETTINGSAsync(
                            getExtrasRequest.ApiKey,
                            getExtrasRequest.ApiPassword,
                            getExtrasRequest.LanguageCode
                        );

                    if (getSettingsResult != null)
                        getSettingsResponse = JsonConvert.DeserializeObject<KOLAYCARSETTINGS>(getSettingsResult.Body.GET_SETTINGSResult);

                }
                getExtrasResponse.Vehicle.FullCredit = getSettingsResponse != null && additionalInformation.Agency.FullCreditPermission.ToBoolNullSafe() ? getSettingsResponse.FULLCREDITACTIVE : false;

                return new ServiceResponseBase
                {
                    Success = getExtrasResponseObject.RETURNCODE == 0,
                    ServiceCode = getExtrasResponseObject.RETURNCODE.ToString(),
                    ServiceMessage = getExtrasResponseObject.MESSAGE,
                    Data = getExtrasResponse
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "KolayCAR servisine ulaşılamadı!",
                ServiceCode = getExtrasResponseObject.RETURNCODE.ToString(),
                ServiceMessage = getExtrasResponseObject.MESSAGE
            };
        }
    }
}
