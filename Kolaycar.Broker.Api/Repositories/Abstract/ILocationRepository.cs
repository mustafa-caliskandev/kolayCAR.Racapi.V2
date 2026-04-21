using KolayCAR.Broker.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Abstract
{
    public interface ILocationRepository : IRepository<Location>
    {
        Task<Location> GetPickupLocation(int pickupLocationId, List<int> agencyLocationIds);
        Task<IEnumerable<Location>> GetActiveLocationList();
        Task<Location> GetLocationByIdAsync(int locationId);
    }
}
