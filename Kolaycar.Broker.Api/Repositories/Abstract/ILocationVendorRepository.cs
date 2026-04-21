using KolayCAR.Broker.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Abstract
{
    public interface ILocationVendorRepository : IRepository<Locationvendor>
    {
        Task<Locationvendor> GetLocationVendor(int pickupLocationId, int vendorId);
        Task<IEnumerable<Locationvendor>> GetLocationVendorByVendorId(int vendorId);
    }
}
