using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class SpecialRequestAgencyRepository : Repository<SpecialRequestAgency>, ISpecialRequestAgencyRepository
    {
        public SpecialRequestAgencyRepository(BrokerContext context) : base(context)
        {
        }
    }
}
