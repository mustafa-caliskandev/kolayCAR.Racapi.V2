using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers
{
    public interface IExtraProvider
    {
        Task<ServiceResponseBase> GetExtras(
            GetExtrasRequest getExtrasRequest,
            Vendor vendor,
            ResponseReservationStepsAdditionalInformation additionalInformation,
            List<ExchangeRates> exchangeRates,
            List<Vehicle> localVehicles,
            List<SubVendor> subVendors,
            bool addProfitMarkup = true,
            bool getAPIPrices = false);
        Task<ServiceResponseBase> GetExtraList(
            Vendor vendor,
            CurrencyTypes currencyType,
            LanguageTypes languageType,
            int rentalDuration);
    }
}
