using KolayCAR.Broker.API.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Repositories.Abstract
{
    public interface IAgencyRepository : IRepository<Agency>
    {
        public Task<IEnumerable<Agency>> GetActiveAgencyList();
    }
}
