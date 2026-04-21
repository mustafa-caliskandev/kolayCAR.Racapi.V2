using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IVendorContactInformationService
    {
        Task<Vendorcontactinformation> GetVendorContactInformation(int locationId, int vendorId);
        Task<List<Vendorcontactinformation>> GetVendorContactInformations();
        Task<List<Vendorcontactinformation>> GetVendorContactInformationsByLocation(int locationId);
        Task<List<Vendorcontactinformation>> GetVendorContactInformationsByVendorId(int vendorId);
    }

    public class VendorContactInformationService : IVendorContactInformationService
    {
        private readonly IVendorContactInformationRepository _vendorContactInformationRepository;
        private readonly ICacheService _cacheService;

        public VendorContactInformationService(IVendorContactInformationRepository vendorContactInformationRepository, ICacheService cacheService)
        {
            _vendorContactInformationRepository = vendorContactInformationRepository;
            _cacheService = cacheService;
        }

        public async Task<Vendorcontactinformation> GetVendorContactInformation(int locationId, int vendorId)
        {
            if (CacheSettings.UseCache)
            {
                var vendorContactInformations = await _cacheService.GetOrCreateAsync(
                    $"{CacheSettings.VendorKey}-VendorContactInformation-{vendorId}",
                    () => _vendorContactInformationRepository.GetVendorContactInformationsByVendorId(vendorId));

                return vendorContactInformations.FirstOrDefault(x => x.Locationid == locationId);
            }

            return await _vendorContactInformationRepository.GetVendorContactInformation(locationId, vendorId);
        }

        public async Task<List<Vendorcontactinformation>> GetVendorContactInformations()
        {
            if (CacheSettings.UseCache)
            {
                var vendorContactInformations = await _cacheService.GetOrCreateAsync(
                    $"{CacheSettings.VendorKey}-VendorContactInformationList",
                    _vendorContactInformationRepository.GetAllAsync);

                return vendorContactInformations.ToList();
            }

            return (await _vendorContactInformationRepository.GetAllAsync()).ToList();
        }

        public async Task<List<Vendorcontactinformation>> GetVendorContactInformationsByLocation(int locationId)
        {
            if (CacheSettings.UseCache)
            {
                var vendorContactInformations = await _cacheService.GetOrCreateAsync(
                    $"{CacheSettings.LocationKey}-VendorContactInformation-{locationId}",
                    () => _vendorContactInformationRepository.GetVendorContactInformationsByLocation(locationId));

                return vendorContactInformations.ToList();
            }

            return (await _vendorContactInformationRepository.GetVendorContactInformationsByLocation(locationId)).ToList();
        }

        public async Task<List<Vendorcontactinformation>> GetVendorContactInformationsByVendorId(int vendorId)
        {
            if (CacheSettings.UseCache)
            {
                var vendorContactInformations = await _cacheService.GetOrCreateAsync(
                    $"{CacheSettings.VendorKey}-VendorContactInformation-{vendorId}",
                    () => _vendorContactInformationRepository.GetVendorContactInformationsByVendorId(vendorId));

                return vendorContactInformations.ToList();
            }

            return (await _vendorContactInformationRepository.GetVendorContactInformationsByVendorId(vendorId)).ToList();
        }
    }
}
