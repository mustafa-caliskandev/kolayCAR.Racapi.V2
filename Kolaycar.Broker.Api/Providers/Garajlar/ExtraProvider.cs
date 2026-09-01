using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Garajlar;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.GarajlarResponseBase;
using KolayCAR.Broker.Infrastructure.Extensions;

namespace KolayCAR.Broker.API.Providers.Garajlar
{
    public class ExtraProvider : IExtraProvider
    {
        private readonly HttpManager _httpManager;
        private readonly AuthProvider _authProvider;
        private readonly VehicleProvider _vehicleProvider;
        public ExtraProvider(Vendor vendor)
        {
            _httpManager = new HttpManager(vendor.APIBaseUrl);
            _authProvider = new AuthProvider(vendor.APIBaseUrl);
            _vehicleProvider = new VehicleProvider(vendor, false);
        }
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var token = await _authProvider.GetTokenHeader(vendor);
            if (token is null)
                return new(null, false, $"{vendor.VendorName} token bilgisi alınamadı!");

            var extraList = await _httpManager.GetAsyncWithModel<ResponseBase<List<GarajlarExtraList>>>("/api/obilet/get-extras", headers: token);

            return extraList?.data?.Count > 0
                ? new(extraList.data.Map(), true)
                : new(null, false, $"{vendor.VendorName} araç listesi alınamadı!");
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var requestLanguageType = getExtrasRequest.LanguageCode.ToEnum<LanguageTypes>();

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await _vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == reservationToken.VehicleCode);

            if (selectedVehicle == null)
                return new (null, false);

            var extras = selectedVehicle.Extras;

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle, vehicles);
            return new (getExtrasResponse, true);
        }
    }
}
