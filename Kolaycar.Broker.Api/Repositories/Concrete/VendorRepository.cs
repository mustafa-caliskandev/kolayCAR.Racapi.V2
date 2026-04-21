using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class VendorRepository : Repository<Vendor>, IVendorRepository
    {
        public VendorRepository(BrokerContext context) : base(context)
        {
        }

        public async Task<Vendor> GetVendorAsync(string apiKey, string apiPassword, string apiClientId, string secretKey, Domain.Models.VendorTypes vendorType)
        {
            return await _dbSet
                   .Where(x => x.Apikey == apiKey
                            && x.Apipassword == apiPassword
                            && x.Vendortype == (int)vendorType
                            && x.Active == true
                            && (string.IsNullOrEmpty(apiClientId) || x.Apiclientid == apiClientId)
                            && (string.IsNullOrEmpty(secretKey) || x.Secretkey == secretKey))
                   .FirstOrDefaultAsync();
        }

    }
}
