using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Reservaway;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Reservaway
{
    public class ExtraProvider : IExtraProvider
    {
        private readonly VehicleProvider _vehicleProvider;

        public ExtraProvider(Vendor vendor)
        {
            _vehicleProvider = new VehicleProvider(vendor, false);
        }

        public async Task<ServiceResponseBase> GetExtras(
            GetExtrasRequest getExtrasRequest,
            Vendor vendor,
            ResponseReservationStepsAdditionalInformation additionalInformation,
            List<ExchangeRates> exchangeRates,
            List<Vehicle> localVehicles,
            List<SubVendor> subVendors,
            bool addProfitMarkup = true,
            bool getAPIPrices = false)
        {
            var reservationToken = additionalInformation?.ReservationToken;
            if (reservationToken == null)
                return new ServiceResponseBase(null, false, $"{vendor.VendorName} ekstra istegi icin reservation token bulunamadi.");

            var detailResult = await _vehicleProvider.GetVehicleDetail(vendor, reservationToken, additionalInformation);
            if (!detailResult.Success)
                return detailResult;

            var detail = detailResult.Data as ReservawayVehicleDetailResult;
            if (detail?.Vehicle == null || detail.ApiVehicle == null)
                return new ServiceResponseBase(null, false, $"{vendor.VendorName} ekstra icin arac detayi alinamadi.");

            var extras = detail.ApiVehicle.extras.Map();
            detail.Vehicle.Extras = extras;

            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            CalculationHelper.SetVehiclePrices(detail.Vehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            extras = ReservationHelper.RemoveZeroPriceExtras(extras);
            return new ServiceResponseBase(new GetExtrasResponse(extras, detail.Vehicle), true);
        }

        public async Task<ServiceResponseBase> GetExtraList(
            Vendor vendor,
            CurrencyTypes currencyType,
            LanguageTypes languageType,
            int rentalDuration)
            => new ServiceResponseBase(null, false, "Reservaway ekstra listesi icin once arac secimi yapilmasi gerekiyor.");
    }
}
