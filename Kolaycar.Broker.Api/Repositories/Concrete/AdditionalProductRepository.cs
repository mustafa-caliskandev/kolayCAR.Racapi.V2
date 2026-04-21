using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Concrete;

public class AdditionalProductRepository : Repository<Additionalproduct>, IAdditionalProductRepository
{
    public AdditionalProductRepository(BrokerContext context) : base(context)
    {
    }

    public async Task<List<Additionalproduct>> GetAdditionalProductsByIdList(IEnumerable<int> idList, int langId)
    {
        if (idList == null || !idList.Any())
            return new List<Additionalproduct>();

        return await _dbSet.Where(e => idList.Contains(e.Productid) && e.Active == true && e.Langid == langId).ToListAsync();
    }
}
