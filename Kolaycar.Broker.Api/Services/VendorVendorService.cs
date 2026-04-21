using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IVendorVendorService
    {
        Task<IEnumerable<VendorVendor>> GetVendorVendorList();
        Task AddRangeAsync(IEnumerable<VendorVendor> vendorVendors);
    }
    public class VendorVendorService : IVendorVendorService
    {
        private readonly IVendorVendorRepository _vendorVendorRepository;
        private readonly ICacheService _cacheService;
        private readonly string VendorsVendorsCacheKey = "VendorsVendorsCacheKey";
        public VendorVendorService(IVendorVendorRepository vendorVendorRepository, ICacheService cacheService)
        {
            _vendorVendorRepository = vendorVendorRepository;
            _cacheService = cacheService;
        }

        public async Task AddRangeAsync(IEnumerable<VendorVendor> vendorVendors)
        {
            await _vendorVendorRepository.AddRangeAsync(vendorVendors);
        }

        public async Task<IEnumerable<VendorVendor>> GetVendorVendorList()
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync($"{CacheSettings.VendorKey} - VendorVendorList", _vendorVendorRepository.GetAllAsync);

            return await _vendorVendorRepository.GetAllAsync();
        }
    }
}
