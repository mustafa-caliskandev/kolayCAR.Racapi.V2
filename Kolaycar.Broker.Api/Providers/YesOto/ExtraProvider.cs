using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kolaycar.Broker.Api.Providers.YesOto
{
    public class ExtraProvider : IExtraProvider
    {
        private readonly HttpManager _httpManager;
        private readonly AuthProvider _authProvider;

        public ExtraProvider(string apiBaseUrl)
        {
            _httpManager = new HttpManager(apiBaseUrl);
            _authProvider = new AuthProvider(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            var accessToken = await _authProvider.GetTokenAsync(vendor);
            if (string.IsNullOrEmpty(accessToken))
            {
                return new ServiceResponseBase(null, false, "Token bilgisi alınamadı!");
            }

            // YesOto API doesn't seem to have a specific standalone Extra endpoint in the provided docs
            // Extras are usually part of the CompleteReservation or Vehicle details
            // Returning an empty success response for now, to comply with the interface
            var mappedExtras = new List<Extra>();

            return new ServiceResponseBase(mappedExtras, true);
        }

        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            // Usually returns a list of Extras for admin panels
            var result = new List<Extra>();
            return await Task.FromResult(new ServiceResponseBase(result, true));
        }
    }
}
