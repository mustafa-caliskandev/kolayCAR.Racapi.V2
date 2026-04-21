using System.Threading.Tasks;
using Vendor = KolayCAR.Broker.API.Models.Vendor;

namespace KolayCAR.Broker.API.Repositories.Abstract
{
    public interface IVendorRepository : IRepository<Vendor>
    {
        Task<Vendor> GetVendorAsync(string apiKey, string apiPassword, string apiClientId, string secretKey, Domain.Models.VendorTypes vendorType);
    }
}
