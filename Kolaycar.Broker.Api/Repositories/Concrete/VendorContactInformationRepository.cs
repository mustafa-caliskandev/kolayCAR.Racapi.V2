using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class VendorContactInformationRepository : Repository<Vendorcontactinformation>, IVendorContactInformationRepository
    {
        public VendorContactInformationRepository(BrokerContext context) : base(context)
        {
        }

        public async Task<Vendorcontactinformation> GetVendorContactInformation(int locationId, int vendorId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Locationid == locationId && x.Vendorid == vendorId);
        }

        public async Task<IEnumerable<Vendorcontactinformation>> GetVendorContactInformationsByLocation(int locationId)
        {
            return await _dbSet.Where(x => x.Locationid == locationId).ToListAsync();
        }

        public async Task<IEnumerable<Vendorcontactinformation>> GetVendorContactInformationsByVendorId(int vendorId)
        {
            return await _dbSet.Where(x => x.Vendorid == vendorId).ToListAsync();
        }
    }
}
