using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface ILocationVendorClosedDateService : IService<Locationvendorcloseddate>
    {
        Task<bool> CheckVendorIsClosedCurrentDate(int vendorId, int locationId, DateTime pickupDate);
    }
    public class LocationVendorClosedDateService : ILocationVendorClosedDateService
    {
        private readonly ILocationVendorClosedDateRepository _locationVendorClosedDateRepository;
        private readonly ICacheService _cacheService;
        private static string LocationVendorClosedDateCacheKey = "LocationVendorClosedDateCacheKey";
        public LocationVendorClosedDateService(ILocationVendorClosedDateRepository locationVendorClosedDateRepository, ICacheService cacheService)
        {
            _locationVendorClosedDateRepository = locationVendorClosedDateRepository;
            _cacheService = cacheService;
        }
        public async Task<IEnumerable<Locationvendorcloseddate>> GetAllAsync()
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync(LocationVendorClosedDateCacheKey, _locationVendorClosedDateRepository.GetAllAsync);

            return await _locationVendorClosedDateRepository.GetAllAsync();
        }

        public async Task<bool> CheckVendorIsClosedCurrentDate(int vendorId, int locationId, DateTime pickupDate)
        {
            var locationVendorClosedDateList = await GetAllAsync();
            var locationVendorClosedDate = locationVendorClosedDateList.Where(x => x.Vendorid == vendorId && x.Locationid == locationId).ToList();
            return ReservationHelper.CheckClosedVendor(locationVendorClosedDate, pickupDate);
        }
    }
}
