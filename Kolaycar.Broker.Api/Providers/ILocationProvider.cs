using KolayCAR.Broker.Domain.Models.Response;
using System.Threading.Tasks;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Providers
{
    public interface ILocationProvider
    {
        Task<ServiceResponseBase> GetLocations(CommonModels.Vendor vendor, int languageId, string locationName = "");
        Task<ServiceResponseBase> GetLocationDetail(CommonModels.Vendor vendor, int languageId, string locationCode);
        string ProviderName { get; }
    }
}
