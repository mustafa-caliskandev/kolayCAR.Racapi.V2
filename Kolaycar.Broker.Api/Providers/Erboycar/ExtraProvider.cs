using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Erboycar;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErboycarProvider = KolayCAR.Broker.API.Providers.Erboycar;

namespace KolayCAR.Broker.API.Providers.Erboycar
{
    public class ExtraProvider : IExtraProvider
    {
        IVehicleProvider vehicleProvider;
        RestManager RestManager { get; set; }

        public ExtraProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var result = await RestManager.GetAsync<List<ErboycarResponseBase.ExtraResponse>>(
                requestPath: "get-extra");

            if (result != null &&
                result.Any())
            {
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = result.Map()?.OrderBy(x => x.ExtraName).ToList()
                };
            }

            return new ServiceResponseBase();
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            vehicleProvider = new VehicleProvider(vendor, false);
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var requestLanguageType = getExtrasRequest.LanguageCode.ToEnum<LanguageTypes>();            

            var result = await RestManager.GetAsync<List<ErboycarResponseBase.ExtraResponse>>(
                requestPath: "get-extra");

            var extras = new List<Extra>();
            if ((bool)result?.Any())
            {
                if (requestCurrencyType != CurrencyTypes.TRY)
                {
                    foreach (var item in result)
                    {
                        if (item.prices.Count > 0)
                        {
                            if (item.prices.Count > 1)
                                item.price_tl = item.prices[1].daily_price;
                            else 
                                item.price_tl = CalculationHelper.CurrencyExchange(exchangeRates, vendor, item.prices[0].daily_price.ToIntNullSafe(), CurrencyTypes.TRY, requestCurrencyType);
                        }
                    }
                }
                extras = result.Map();           
            }
            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);
            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == reservationToken.VehicleCode);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle);
            return new ServiceResponseBase(getExtrasResponse, true);           
        }
    }
}
