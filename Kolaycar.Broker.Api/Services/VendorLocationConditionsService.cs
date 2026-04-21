using KolayCAR.Broker.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IVendorLocationConditionsService
    {
        Task<List<VendorLocationCondition>> GetAll();
    }
    public class VendorLocationConditionsService : IVendorLocationConditionsService
    {
        private readonly BrokerContext _context;
        public VendorLocationConditionsService(BrokerContext context)
        {
            _context = context;
        }
        public async Task<List<VendorLocationCondition>> GetAll()
        {
            var list = _context.VendorLocationConditions.ToList();

            return list;
        }
    }
}
