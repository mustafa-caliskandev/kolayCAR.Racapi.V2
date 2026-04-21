using KolayCAR.Broker.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Abstract
{
    public interface IVendorContactInformationRepository : IRepository<Vendorcontactinformation>
    {
        Task<Vendorcontactinformation> GetVendorContactInformation(int locationId, int vendorId);
        Task<IEnumerable<Vendorcontactinformation>> GetVendorContactInformationsByLocation(int locationId);
        Task<IEnumerable<Vendorcontactinformation>> GetVendorContactInformationsByVendorId(int vendorId);
    }
}
