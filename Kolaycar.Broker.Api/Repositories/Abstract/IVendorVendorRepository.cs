using KolayCAR.Broker.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Abstract
{
    public interface IVendorVendorRepository : IRepository<VendorVendor>
    {
        Task<IEnumerable<VendorVendor>> GetVendorVendorListByVendorIdAsync(int vendorId);
    }
}
