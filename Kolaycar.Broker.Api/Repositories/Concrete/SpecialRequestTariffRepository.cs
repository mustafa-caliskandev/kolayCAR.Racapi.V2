using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class SpecialRequestTariffRepository : Repository<SpecialRequestTariff>, ISpecialRequestTariffRepository
    {
        public SpecialRequestTariffRepository(BrokerContext context) : base(context)
        {
        }
    }
}
