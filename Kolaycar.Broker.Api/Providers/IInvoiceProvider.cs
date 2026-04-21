using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers
{
    public interface IInvoiceProvider
    {
        Task<ServiceResponseBase> SendReservationInvoice(Configurations configurations, ReservationInvoice invoice);
        Task<ServiceResponseBase> SendCancelReservationInvoice(Configurations configurations, ReservationInvoice invoice);
    }
}
