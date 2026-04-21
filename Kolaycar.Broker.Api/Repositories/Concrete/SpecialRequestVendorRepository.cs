using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class SpecialRequestVendorRepository : Repository<SpecialRequestVendor>, ISpecialRequestVendorRepository
    {
        public SpecialRequestVendorRepository(BrokerContext context) : base(context)
        {
        }
    }
}
