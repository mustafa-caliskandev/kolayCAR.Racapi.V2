using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class AgencyRepository : Repository<Agency>, IAgencyRepository
    {
        public AgencyRepository(BrokerContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Agency>> GetActiveAgencyList() => await _dbSet.Where(e => e.Active == true).ToListAsync();
    }
}
