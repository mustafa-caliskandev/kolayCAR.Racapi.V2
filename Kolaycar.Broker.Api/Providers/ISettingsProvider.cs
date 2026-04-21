using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace KolayCAR.Broker.API.Providers
{
    public interface ISettingsProvider
    {
        Task<ServiceResponseBase> GetSettings(GetSettingsRequest getSettingsRequest, Vendor vendor, ReservationToken reservationToken, List<ExchangeRates> exchangeRates);
        Task<ServiceResponseBase> GetSettingsFromVendorApi(Vendor vendor);
        //Task<ServiceResponseBase> GetScoreFromVendorApi(Vendor vendor, int vendorId, int locationId);
    }
}
