using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.API.Repositories.Abstract;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class LocationVendorClosedDateRepository : Repository<Locationvendorcloseddate>, ILocationVendorClosedDateRepository
    {
        public LocationVendorClosedDateRepository(BrokerContext context) : base(context)
        {
        }
    }
}
