using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Reservaway
{
    public class SummaryProvider : ISummaryProvider
    {
        private readonly IExtraProvider _extraProvider;

        public SummaryProvider(Vendor vendor)
        {
            _extraProvider = new ExtraProvider(vendor);
        }

        public async Task<ServiceResponseBase> GetSummary(
            GetSummaryRequest getSummaryRequest,
            Vendor vendor,
            ResponseReservationStepsAdditionalInformation additionalInformation,
            List<ExchangeRates> exchangeRates,
            List<Vehicle> localVehicles,
            List<SubVendor> subVendors)
        {
            var reservationToken = additionalInformation?.ReservationToken;
            if (reservationToken == null)
                return new ServiceResponseBase(null, false, $"{vendor.VendorName} ozet istegi icin reservation token bulunamadi.");

            var getExtrasRequest = new GetExtrasRequest
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

            var getExtrasResponse = await _extraProvider.GetExtras(
                getExtrasRequest,
                vendor,
                additionalInformation,
                exchangeRates,
                localVehicles,
                subVendors);

            if (!getExtrasResponse.Success)
                return getExtrasResponse;

            var extrasResponse = getExtrasResponse.Data as GetExtrasResponse;
            if (extrasResponse?.Vehicle == null)
                return new ServiceResponseBase(null, false, $"{vendor.VendorName} ozet verisi olusturulamadi.");

            var requestCurrencyType = getSummaryRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var vehicleData = extrasResponse.Vehicle;
            var extrasData = ReservationHelper.ExtraToReservationExtra(extrasResponse.Extras ?? new List<Extra>(), getSummaryRequest.ExtraList);

            CalculationHelper.SetVehiclePrices(vehicleData, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);

            return new ServiceResponseBase
            {
                Success = true,
                Data = new GetSummaryResponse
                {
                    Vehicle = vehicleData,
                    Extras = extrasData
                }
            };
        }
    }
}
