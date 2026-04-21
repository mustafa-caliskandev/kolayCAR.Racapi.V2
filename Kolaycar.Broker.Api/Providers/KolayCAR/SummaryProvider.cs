using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.KolayCAR;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Resws;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Resws.ServiceSoapClient;
using KolayCARHelper = KolayCAR.Broker.API.Helpers.KolayCAR;

namespace KolayCAR.Broker.API.Providers.KolayCAR
{
    public class SummaryProvider : ISummaryProvider
    {
        private readonly ServiceSoapClient _kolayCARService;

        public SummaryProvider(Vendor vendor)
        {
            _kolayCARService = new ServiceSoapClient(EndpointConfiguration.ServiceSoap, Helpers.KolayCAR.ReservationHelper.GetSoapRemoteAddress(vendor.APIBaseUrl));
        }

        public async Task<ServiceResponseBase> GetSummary(GetSummaryRequest getSummaryRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors)
        {
            var reservationToken = additionalInformation.ReservationToken;
            var formattedExtraList = KolayCARHelper.ExtraHelper.FormatExtras(getSummaryRequest.ExtraList);
            int vehicleId = reservationToken.VehicleCode.Split('-')[0].ToIntNullSafe();

            var getSummaryResult = await _kolayCARService.GET_SUMMARY1Async(
                vendor.ApiKey,
                vendor.ApiPassword,
                getSummaryRequest.LanguageCode,
                reservationToken.BaseVendorRequestCurrencyType.ToString(),
                Convert.ToInt32(reservationToken.APIPickupLocationCode),
                Convert.ToInt32(reservationToken.APIReturnLocationCode),
                getSummaryRequest.PickupDate,
                getSummaryRequest.ReturnDate,
                getSummaryRequest.PickupTime,
                getSummaryRequest.ReturnTime,
                reservationToken.APIVendorId,
                vehicleId,
                formattedExtraList,
                getSummaryRequest.UserToken,
                getSummaryRequest.CouponCode,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);

            var getSummaryResponseObject = JsonConvert.DeserializeObject<KolayCARResponseBase>(getSummaryResult.GET_SUMMARY_V2Result);

            if (getSummaryResponseObject.RETURNCODE == 0)
            {
                var extrasData = new List<ReservationExtra>();
                var requestCurrencyType = getSummaryRequest.CurrencyCode.ToEnum<CurrencyTypes>();
                extrasData = ReservationHelper.ExtraToReservationExtra(getSummaryResponseObject.EXTRAS.Map(), getSummaryRequest.ExtraList);
                var vehiclesData = CalculationHelper.CalculateFinalVehiclePrices(vendor, getSummaryResponseObject.VEHICLES[0].Map(additionalInformation, vendor), additionalInformation.Agency);

                var getSummaryResponse = new GetSummaryResponse
                {
                    Extras = extrasData,
                    Vehicle = vehiclesData
                };

                CalculationHelper.SetExtraPrices(getSummaryResponse.Extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup: true, getAPIPrices: false, reservationToken.BaseVendorRequestCurrencyType);
                CalculationHelper.SetVehiclePrices(getSummaryResponse.Vehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);

                if (vendor.VehicleMappingActive)
                {
                    getSummaryResponse.Vehicle = VehicleHelper.MapLocalVehicle(getSummaryResponse.Vehicle, localVehicles.Where(x => x.VehicleCode == getSummaryResponse.Vehicle.VehicleCode).FirstOrDefault());
                }

                return new ServiceResponseBase
                {
                    Success = getSummaryResponseObject.RETURNCODE == 0,
                    ServiceCode = getSummaryResponseObject.RETURNCODE.ToString(),
                    ServiceMessage = getSummaryResponseObject.MESSAGE,
                    Data = getSummaryResponse
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "KolayCAR servisine ulaşılamadı!",
                ServiceCode = getSummaryResponseObject.RETURNCODE.ToString(),
                ServiceMessage = getSummaryResponseObject.MESSAGE
            };
        }
    }
}
