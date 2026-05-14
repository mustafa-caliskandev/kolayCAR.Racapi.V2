using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class VendorVendorRepository : Repository<VendorVendor>, IVendorVendorRepository
    {
        public VendorVendorRepository(BrokerContext context) : base(context)
        {
        }

        public async Task<IEnumerable<VendorVendor>> GetVendorVendorListByVendorIdAsync(int vendorId) => await _dbSet.Where(e => e.VendorId == vendorId).ToListAsync();
    }
}
