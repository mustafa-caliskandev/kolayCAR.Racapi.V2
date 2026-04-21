using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class BultenLogRepository : Repository<BultenLog>, IBultenLogRepository
    {
        public BultenLogRepository(BrokerContext context) : base(context)
        {
        }
    }
}
