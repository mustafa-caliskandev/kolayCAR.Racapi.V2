using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IVendorOfficeService
    {
        Task<VendorOffice> GetVendorOffice(int vendorId, int pickupLocationId);
    }
    public class VendorOfficeService : IVendorOfficeService
    {
        private readonly IVendorOfficeRepository _vendorOfficeRepository;
        private readonly ICacheService _cacheService;
        public VendorOfficeService(IVendorOfficeRepository vendorOfficeRepository, ICacheService cacheService)
        {
            _vendorOfficeRepository = vendorOfficeRepository;
            _cacheService = cacheService;
        }
        public async Task<VendorOffice> GetVendorOffice(int vendorId, int locationId)
        {
            if (CacheSettings.UseCache)
            {
                var vendorOfficeList = await _cacheService.GetOrCreateAsync($"{CacheSettings.VendorKey}-VendorOfficeList", _vendorOfficeRepository.GetAllAsync);
                return vendorOfficeList.FirstOrDefault(e => e.VendorId == vendorId && e.LocationId == locationId);
            }

            return await _vendorOfficeRepository.GetVendorOffice(vendorId, locationId);
        }
    }
}
