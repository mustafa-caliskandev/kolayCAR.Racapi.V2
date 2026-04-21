using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers
{
    public interface IVehicleProvider
    {
        Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest,
                                              Vendor vendor,
                                              ResponseReservationStepsAdditionalInformation additionalInformation,
                                              List<ExchangeRates> exchangeRates,
                                              List<Vehicle> localVehicles,
                                              List<SubVendor> subVendors,
                                              CurrencyTypes baseVendorRequestCurrencyType,
                                              List<ProfitMarkup> profitMarkups = null
                                              );
        Task<ServiceResponseBase> GetVehicleList(Vendor vendor);
    }
}
