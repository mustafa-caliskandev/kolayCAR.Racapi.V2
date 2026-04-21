using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class VendorOfficeRepository : Repository<VendorOffice>, IVendorOfficeRepository
    {
        public VendorOfficeRepository(BrokerContext context) : base(context)
        {
        }
        public async Task<VendorOffice> GetVendorOffice(int vendorId, int locationId) => await _dbSet.FirstOrDefaultAsync(o => o.VendorId == vendorId && o.LocationId == locationId);
    }
}
