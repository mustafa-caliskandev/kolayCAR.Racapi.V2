using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class AgencyVendorProfitMarkupRepository : Repository<Agencyvendorprofitmarkup>, IAgencyVendorProfitMarkupRepository
    {
        public AgencyVendorProfitMarkupRepository(BrokerContext context) : base(context)
        {
        }
    }
}
