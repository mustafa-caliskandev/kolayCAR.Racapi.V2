using KolayCAR.Broker.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Abstract;

public interface IAdditionalProductRepository : IRepository<Additionalproduct>
{
    Task<List<Additionalproduct>> GetAdditionalProductsByIdList(IEnumerable<int> idList, int langId);
}
