using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Beto
{
    public class SummaryProvider : ISummaryProvider
    {
        IExtraProvider extraProvider;
        public async Task<ServiceResponseBase> GetSummary(GetSummaryRequest getSummaryRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors)
        {

            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getSummaryRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            extraProvider = new ExtraProvider(vendor.APIBaseUrl);

            GetExtrasRequest getExtrasRequest = new GetExtrasRequest
            {
                VendorType = getSummaryRequest.VendorType,
                ApiKey = vendor.ApiKey,
                ApiPassword = vendor.ApiPassword,
                LanguageCode = getSummaryRequest.LanguageCode,
                CurrencyCode = getSummaryRequest.CurrencyCode,
                PickupLocationId = getSummaryRequest.PickupLocationId,
                ReturnLocationId = getSummaryRequest.ReturnLocationId,
                PickupDate = getSummaryRequest.PickupDate,
                ReturnDate = getSummaryRequest.ReturnDate,
                PickupTime = getSummaryRequest.PickupTime,
                ReturnTime = getSummaryRequest.ReturnTime,
                UserToken = getSummaryRequest.UserToken,
                CouponCode = getSummaryRequest.CouponCode
            };

            var getExtrasResponse = await extraProvider.GetExtras(getExtrasRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors);

            var extrasResponse = getExtrasResponse.Data as GetExtrasResponse;
            var vehicleData = extrasResponse.Vehicle;
            var extrasData = ReservationHelper.ExtraToReservationExtra(extrasResponse.Extras, getSummaryRequest.ExtraList);

            CalculationHelper.SetVehiclePrices(vehicleData, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);

            var getSummaryResponse = new GetSummaryResponse
            {
                Vehicle = vehicleData,
                Extras = extrasData
            };

            return new ServiceResponseBase
            {
                Success = getSummaryResponse.Vehicle != null,
                Data = getSummaryResponse
            };
        }
    }
}
