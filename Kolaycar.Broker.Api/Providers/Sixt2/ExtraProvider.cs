using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Sixt2;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Sixt2
{
    public class ExtraProvider : IExtraProvider
    {
        IVehicleProvider _vehicleProvider { get; set; }
        HttpManager _httpManager;
        AuthProvider _authProvider;
        public ExtraProvider(Vendor vendor, ICacheService cacheService)
        {
            _vehicleProvider = new VehicleProvider(vendor, cacheService, false);
            _httpManager = new HttpManager(vendor.APIBaseUrl);
            _authProvider = new AuthProvider(vendor.APIBaseUrl, cacheService);
        }
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            string filePath = Path.Combine(@"Docs\Sixt\ExtraList.json");
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
                var dataList = JsonConvert.DeserializeObject<SixtExtras>(jsonContent);
                if (dataList?.extras?.Count > 0)
                    return new(dataList.extras.Map(), true, "");
            }
            return new(null, false, "Extra listesine ulaşılamadı!");
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var requestLanguageType = getExtrasRequest.LanguageCode.ToEnum<LanguageTypes>();
            var token = JsonConvert.DeserializeObject<Dictionary<string, object>>(reservationToken.APIReferenceCode2);

            var extras = new List<Extra>();

            if (token != null)
            {
                var extraList = await _httpManager.GetAsyncWithModel<SixtResponseBase<SixtGetExtrasResponse>>("/api/v1/reservations/extras", parameters: GetParameters(reservationToken, additionalInformation), headers: token);

                if (extraList?.result?.extras?.Count > 0)
                    extras = extraList?.result?.extras?.Map();
            }

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await _vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == reservationToken.VehicleCode);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle, vehicles);
            return new(getExtrasResponse, true);
        }

        private IDictionary<string, object> GetParameters(ReservationToken reservationToken, ResponseReservationStepsAdditionalInformation additionalInformation)
        {
            return new Dictionary<string, object> {
                { "unid", reservationToken.APIReferenceCode},
                { "vehicle_group", reservationToken.VehicleCode.Split('|')[0] },
                { "station_code", additionalInformation.APIPickupLocationCode.Split("~")[1] }
            };
        }
    }
}
