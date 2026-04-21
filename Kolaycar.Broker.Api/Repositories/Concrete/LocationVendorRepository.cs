using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class LocationVendorRepository : Repository<Locationvendor>, ILocationVendorRepository
    {
        public LocationVendorRepository(BrokerContext context) : base(context)
        {
        }

        public async Task<Locationvendor> GetLocationVendor(int pickupLocationId, int vendorId)
        {
            return await _dbSet.Where(x => x.Locallocationid == pickupLocationId &&
                        x.Active == true &&
                        x.Vendorid == vendorId)
                        .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Locationvendor>> GetLocationVendorByVendorId(int vendorId)
        {
            return await _dbSet.Where(e => e.Vendorid == vendorId).ToListAsync();
        }
    }
}
