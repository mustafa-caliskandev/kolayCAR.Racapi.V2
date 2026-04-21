using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Pandora;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PandoraProvider = KolayCAR.Broker.API.Providers.Pandora;

namespace KolayCAR.Broker.API.Providers.Pandora
{
    public class ExtraProvider : IExtraProvider
    {
        IVehicleProvider vehicleProvider;
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        public ExtraProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
            AuthProvider = new AuthProvider(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var auth = await AuthProvider.GetAccessToken(vendor);

            if (auth != null)
            {
                var result = await RestManager.GetAsync<List<PandoraResponseBase.PandoraAddition>>(
                    requestPath: $"tr/api/additions",
                    headers: AuthProvider.CreateAuthHeader(auth.access_token));

                return new ServiceResponseBase
                {
                    Success = result != null,
                    Data = result.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Kimlik doğrulama işlemi başarısız!",
            };
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();

            vehicleProvider = new VehicleProvider(vendor, false);

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == additionalInformation.ReservationToken.VehicleCode);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            var extras = selectedVehicle.Extras;

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle);
            return new ServiceResponseBase(getExtrasResponse, true);
        }
    }

    public class PostBookingsCreateRequets
    {
        public bool BookAsCommissioner { get; set; }
        public string Currency { get; set; }
        public int CarCategoryId { get; set; }
        public int PricelistId { get; set; }
        public int OfficeOutId { get; set; }
        public int OfficeInId { get; set; }
        public string DateOut { get; set; }
        public string DateIn { get; set; }
    }
}
