using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;

namespace KolayCAR.Broker.API.Repositories.Concrete;

public class VendorLocationDeliveryTypeRepository : Repository<VendorLocationDeliveryType>, IVendorLocationDeliveryTypeRepository
{
    public VendorLocationDeliveryTypeRepository(BrokerContext context) : base(context)
    {
    }
}
