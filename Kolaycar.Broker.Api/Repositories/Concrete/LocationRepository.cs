using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class LocationRepository : Repository<Location>, ILocationRepository
    {
        public LocationRepository(BrokerContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Location>> GetActiveLocationList()
        {
            return await _dbSet.Where(e => e.Active == true).ToListAsync();
        }

        public async Task<Location> GetPickupLocation(int pickupLocationId, List<int> agencyLocationIds)
        {
            return await _dbSet.Where(x => x.Id == pickupLocationId && x.Active == true && !agencyLocationIds.Contains(x.Id))
        .FirstOrDefaultAsync();
        }
        public async Task<Location> GetLocationByIdAsync(int locationId)
        {
            return await _dbSet.Where(x => x.Id == locationId).FirstOrDefaultAsync();
        }
    }
}
