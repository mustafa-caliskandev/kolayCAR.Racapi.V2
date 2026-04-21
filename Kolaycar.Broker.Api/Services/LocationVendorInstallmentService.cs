using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface ILocationVendorInstallmentService
    {
        Task<LocationVendorInstallment> GetLocationVendorInstallmentAsync(int locationId, int vendorId);
    }
    public class LocationVendorInstallmentService : ILocationVendorInstallmentService
    {
        private readonly BrokerContext _context;
        public LocationVendorInstallmentService(BrokerContext brokerContext)
        {
            _context = brokerContext;
        }
        public async Task<LocationVendorInstallment> GetLocationVendorInstallmentAsync(int locationId, int vendorId)
        {
            var results = await _context.LocationVendorInstallments.ToListAsync();

            var filteredData = results.FirstOrDefault(r => r.LocationId == locationId && r.VendorId == vendorId);
            if (filteredData != null && filteredData.Id > 0)
            {
                return filteredData;
            }

            filteredData = results.FirstOrDefault(r => r.LocationId == locationId);
            return filteredData != null && filteredData.Id > 0 ? filteredData : null;
        }
    }
}
