using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers;
using KolayCAR.Broker.API.Mappers.KolayCARBroker;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerMapper = KolayCAR.Broker.API.Mappers.KolayCARBroker;

namespace KolayCAR.Broker.API.Providers.KolayCARBroker
{
    public class SummaryProvider : ISummaryProvider
    {
        HttpManager HttpManager { get; set; }
        AuthProvider AuthProvider { get; set; }

        public SummaryProvider(string apiBaseUrl)
        {
            HttpManager = new HttpManager(apiBaseUrl);
            AuthProvider = new AuthProvider(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetSummary(GetSummaryRequest getSummaryRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors)
        {
            var auth = await AuthProvider.GetJWT(vendor.ApiKey, vendor.ApiPassword, EncryptionHelper.Encrypt(vendor.ApiPassword));

            if (auth != null)
            {
                var user = auth.Data as User;

                var result = await HttpManager.GetAsync<GetSummaryResponse>(
                      requestPath: "summary",
                      parameters: CreateBrokerGetSummaryRequestParameters(getSummaryRequest, additionalInformation.ReservationToken, additionalInformation),
                      headers: AuthProvider.CreateAuthHeader(user.Token));

                if (result.Success)
                {
                    var getSummaryData = result.Data as GetSummaryResponse;
                    var reservationToken = additionalInformation.ReservationToken;
                    var requestCurrencyType = getSummaryRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                    var mappedExtras = !vendor.UseBrokerConfigurations ? BrokerMapper.ExtraMapper.ToExtraMapper(getSummaryData.Extras) : getSummaryData.Extras.MapExtras();

                    var extrasData = ReservationHelper.ExtraToReservationExtra(mappedExtras, ReservationHelper.ChangeExtraCodeAndExtraId(getSummaryRequest.ExtraList), useBrokerConfigurations: vendor.UseBrokerConfigurations);
                    var vehiclesData = CalculationHelper.CalculateFinalVehiclePrices(vendor, !vendor.UseBrokerConfigurations ? getSummaryData.Vehicle.Map(additionalInformation) : getSummaryData.Vehicle, additionalInformation.Agency);

                    vehiclesData.ExtraPrice = ReservationHelper.GetTotalExtraAmount(extrasData, additionalInformation.RentalDuration);

                    var getSummaryResponse = new GetSummaryResponse
                    {
                        Extras = extrasData,
                        Vehicle = vehiclesData
                    };

                    CalculationHelper.SetExtraPrices(getSummaryResponse.Extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup: true, getAPIPrices: false, reservationToken.BaseVendorRequestCurrencyType);
                    CalculationHelper.SetVehiclePrices(getSummaryResponse.Vehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration, useVendorProps: !vendor.UseBrokerConfigurations);

                    if (vendor.VehicleMappingActive)
                    {
                        getSummaryResponse.Vehicle = VehicleHelper.MapLocalVehicle(getSummaryResponse.Vehicle, localVehicles.Where(x => x.VehicleCode == getSummaryResponse.Vehicle.VehicleId.ToStringNullSafe()).FirstOrDefault(), useBaseVehiclePropsFromVendorAPI: true);
                    }

                    return new ServiceResponseBase
                    {
                        Success = result.Success,
                        Message = result.Message,
                        ServiceMessage = result.Message,
                        Data = getSummaryResponse
                    };
                }

                return new ServiceResponseBase
                {
                    Success = result.Success,
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

        private Dictionary<string, object> CreateBrokerGetSummaryRequestParameters(GetSummaryRequest getSummaryRequest, ReservationToken reservationToken, ResponseReservationStepsAdditionalInformation additionalInformation)
        {
            return new Dictionary<string, object>()
            {
                { "languageCode",  getSummaryRequest.LanguageCode},
                { "currencyCode",  reservationToken.BaseVendorRequestCurrencyType.ToString()},
                { "pickupLocationId",  additionalInformation.APIPickupLocationCode},
                { "returnLocationId",  additionalInformation.APIReturnLocationCode},
                { "pickupDate", getSummaryRequest.PickupDate},
                { "returnDate", getSummaryRequest.ReturnDate},
                { "pickupTime", getSummaryRequest.PickupTime},
                { "returnTime", getSummaryRequest.ReturnTime},
                { "extraList", getSummaryRequest.ExtraList},
                { "reservationToken",  reservationToken.APIReferenceCode}
            };
        }
    }
}
