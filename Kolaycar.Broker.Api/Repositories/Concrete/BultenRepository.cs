using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class BultenRepository : Repository<Bulten>, IBultenRepository
    {
        public BultenRepository(BrokerContext context) : base(context)
        {
        }

        public async Task<Bulten> GetByEmail(string customerEmail)
        {
            return await _dbSet.Where(e => e.Email == customerEmail).FirstOrDefaultAsync();
        }
    }
}
