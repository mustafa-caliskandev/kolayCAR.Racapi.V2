using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class ResTokenRepository : Repository<Restoken>, IResTokenRepository
    {
        public ResTokenRepository(BrokerContext context) : base(context)
        {
        }

        public async Task<Restoken> GetRestokenByUniqueId(string uniqueId) => await _dbSet.FirstOrDefaultAsync(e => e.Uniqueid == uniqueId);
    }
}
