using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers;
using KolayCAR.Broker.API.Mappers.Eganis;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Eganis
{
    public class ExtraProvider : IExtraProvider
    {
        VehicleProvider _vehicleProvider;
        public ExtraProvider(Vendor vendor) 
        {
            _vehicleProvider = new VehicleProvider(vendor, false);
        }
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {

            FileStream stream = File.OpenRead($"Docs\\Eganis\\{vendor.VendorName}\\extra.json");

            var result = await JsonSerializer.DeserializeAsync<EganisResponseBase.ExtraResponse.ExtraList>(stream);

            if(result?.Extras?.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Data = result.Extras.Map(vendor),
                    Success = true,
                };
            }


            return new ServiceResponseBase
            {
                Success = false,
                Message = $"Eganis ({vendor.VendorName}) servisine ulaşılamadı"
            };
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();

            _vehicleProvider = new VehicleProvider(vendor, false);

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);
            getVehiclesRequest.ReservationToken = reservationToken.ToJson();

            var getVehiclesResponse = await _vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleId == additionalInformation.VehicleId);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            var extras = selectedVehicle.Extras;
            selectedVehicle.Extras = null;

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle);
            return new ServiceResponseBase(getExtrasResponse, true);
        }
    }
}
