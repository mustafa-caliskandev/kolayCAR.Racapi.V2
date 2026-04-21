using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class SpecialRequestLocationRepository : Repository<SpecialRequestLocation>, ISpecialRequestLocationRepository
    {
        public SpecialRequestLocationRepository(BrokerContext context) : base(context)
        {
        }
    }
}
