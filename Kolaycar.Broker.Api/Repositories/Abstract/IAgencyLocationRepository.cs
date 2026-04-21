using KolayCAR.Broker.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Abstract
{
    public interface IAgencyLocationRepository : IRepository<Agencylocation>
    {
        Task<List<int>> GetAgencyLocationsId(long agencyId);
    }
}
