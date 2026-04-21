using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface ISpecialRequestService
    {
        Task<IEnumerable<SpecialRequest>> GetSpecialRequestByFilter(ResponseReservationStepsAdditionalInformation additionalInformation, float vehiclePrice, int categoryId);
    }
    public class SpecialRequestService : ISpecialRequestService
    {
        private readonly ISpecialRequestRepository _specialRequestRepository;
        private readonly ICacheService _cacheService;
        public SpecialRequestService(ISpecialRequestRepository specialRequestRepository, ICacheService cacheService)
        {
            _specialRequestRepository = specialRequestRepository;
            _cacheService = cacheService;
        }
        public async Task<IEnumerable<SpecialRequest>> GetSpecialRequestByFilter(ResponseReservationStepsAdditionalInformation additionalInformation, float vehiclePrice, int categoryId = 0)
        {
            if (categoryId == 0)
                categoryId++;

            if (CacheSettings.UseCache)
            {
                var specialRequests = await _cacheService.GetOrCreateAsync(CacheSettings.SpecialRequestKey, () => _specialRequestRepository.GetAllSpecialRequestsWithRelationsAsync(), TimeSpan.FromHours(12));

                return ApplyFilter(specialRequests, additionalInformation, vehiclePrice, categoryId);
            }

            var specialRequest = await _specialRequestRepository.GetSpecialRequestByFilter(additionalInformation.Vendor.VendorId, (int)additionalInformation.Agency.AgencyId, additionalInformation.PickupLocationId, DateTime.Now, additionalInformation.PickupDateTime, additionalInformation.RentalDuration, vehiclePrice, categoryId);

            return specialRequest;
        }

        private IEnumerable<SpecialRequest> ApplyFilter(IEnumerable<SpecialRequest> specialRequests, ResponseReservationStepsAdditionalInformation additionalInformation, float vehiclePrice, int categoryId)
        {
            return specialRequests
                  .Where(e =>
                            !e.Vendors.Any(v => v.VendorId == additionalInformation.Vendor.VendorId) &&
                            !e.Agencies.Any(a => a.AgencyId == additionalInformation.Agency.AgencyId) &&
                            !e.Locations.Any(l => l.LocationId == additionalInformation.PickupLocationId) &&
                            (e.Categories == null || e.Categories.Count == 0 || e.Categories.Any(c => c.CategoryId == categoryId)) &&
                            (e.ReservationStartDate == null || e.ReservationStartDate < DateTime.Now) &&
                            (e.ReservationEndDate == null || e.ReservationEndDate > DateTime.Now) &&
                            (e.MinimumDay == null || e.MinimumDay <= additionalInformation.RentalDuration) &&
                            (e.MaximumDay == null || e.MaximumDay >= additionalInformation.RentalDuration) &&
                            (e.PickupStartDate == null || e.PickupStartDate < additionalInformation.PickupDateTime) &&
                            (e.PickupEndDate == null || e.PickupEndDate > additionalInformation.PickupDateTime) &&
                            (e.ReservationStartDate == null || e.ReservationStartDate < DateTime.Now) &&
                            (e.ReservationEndDate == null || e.ReservationEndDate > DateTime.Now) &&
                            (e.MaximumAmount == null || e.MaximumAmount >= vehiclePrice) &&
                            (e.MinimumAmount == null || e.MinimumAmount <= vehiclePrice)
                            &&
                            e.Active
                            ).GroupBy(e => e.AdditionalProductId)
                    .Select(g => g
                .OrderByDescending(x => x.Priority)
                .FirstOrDefault())
            .ToList();
        }
    }
}
