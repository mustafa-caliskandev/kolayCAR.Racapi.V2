using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class ConfigurationRepository : Repository<Parametre>, IConfigurationRepository
    {
        public ConfigurationRepository(BrokerContext context) : base(context)
        {
        }
    }
}
