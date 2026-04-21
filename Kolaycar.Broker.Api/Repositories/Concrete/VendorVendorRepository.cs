using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class VendorVendorRepository : Repository<VendorVendor>, IVendorVendorRepository
    {
        public VendorVendorRepository(BrokerContext context) : base(context)
        {
        }
    }
}
