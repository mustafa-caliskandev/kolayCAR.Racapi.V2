using KolayCAR.Broker.API.Models;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Abstract
{
    public interface IVendorOfficeRepository : IRepository<VendorOffice>
    {
        Task<VendorOffice> GetVendorOffice(int vendorId, int locationId);
    }
}
