using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.FindeksRequestBase;


namespace KolayCAR.Broker.API.Providers
{
    public interface IFindeksProvider
    {
        public Task<ServiceResponseBase> IsActiveFindeksReportExists(GetIsActiveFindeksReportRequest getActiveFindeksReportRequest, Vendor vendor);
        public Task<ServiceResponseBase> FindeksPhoneIdListRequest(GetPhoneIdListRequest getPhoneIdListRequest, Vendor vendor);
        public Task<ServiceResponseBase> FindeksReportRequest(GetFindeksReportRequest getFindeksReportRequest, Vendor vendor);
        public Task<ServiceResponseBase> FindeksPinRenew(CreatePinRenewRequest createPinRenewRequest, Vendor vendor);
        public Task<ServiceResponseBase> FindeksPinConfirm(ConfirmPinRequest confirmPinRequest, Vendor vendor);
    }
}
