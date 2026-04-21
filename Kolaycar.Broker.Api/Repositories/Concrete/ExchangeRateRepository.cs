using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class ExchangeRateRepository : Repository<Exchangerates>, IExchangeRateRepository
    {
        public ExchangeRateRepository(BrokerContext context) : base(context)
        {
        }
    }
}
