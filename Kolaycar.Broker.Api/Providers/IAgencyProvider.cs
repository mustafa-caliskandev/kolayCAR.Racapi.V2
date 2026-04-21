using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers
{
    public interface IAgencyProvider
    {
        Task<ServiceResponseBase> GetAgency(GetAgenciesRequest getAgenciesRequest, Vendor vendor, ReservationToken reservationToken, Agency agency);
    }
}
