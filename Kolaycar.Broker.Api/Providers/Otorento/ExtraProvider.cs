using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Otorento
{
    public class ExtraProvider : IExtraProvider
    {
        private string apiBaseUrl;
        private IVehicleProvider _vehicleProvider;
        public ExtraProvider(string apiBaseUrl)
        {
            this.apiBaseUrl = apiBaseUrl;
        }

        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            return new ServiceResponseBase { Data = null, Success = true };
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();

            _vehicleProvider = new VehicleProvider(vendor, false);

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await _vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleId == additionalInformation.VehicleId && x.VehicleCode == additionalInformation.ReservationToken.VehicleCode);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            var extras = selectedVehicle.Extras;

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle);
            return new ServiceResponseBase(getExtrasResponse, true);
        }
    }
}
