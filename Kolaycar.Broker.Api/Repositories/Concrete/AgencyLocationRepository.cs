using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class AgencyLocationRepository : Repository<Agencylocation>, IAgencyLocationRepository
    {
        public AgencyLocationRepository(BrokerContext context) : base(context)
        {
        }

        public async Task<List<int>> GetAgencyLocationsId(long agencyId)
        {
            return await _dbSet.Where(e => e.Agencyid == agencyId).Select(x => x.Locationid).ToListAsync();
        }

    }
}
