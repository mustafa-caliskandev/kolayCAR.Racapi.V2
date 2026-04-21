using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class SubVendorRepository : Repository<Subvendor>, ISubVendorRepository
    {
        public SubVendorRepository(BrokerContext context) : base(context)
        {
        }
    }
}
