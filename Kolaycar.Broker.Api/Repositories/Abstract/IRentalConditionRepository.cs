using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Abstract
{
    public interface IRentalConditionRepository
    {
        Task<ServiceResponseBase> GetRentalConditions(int vendorId, LanguageTypes languageType);
    }
}
